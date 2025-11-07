using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.EntityFrameworkCore;
using appReservas.Models; // tus modelos
using appReservas.Data;   // ruta a tu ApplicationDbContext (ajusta según tu proyecto)

namespace appReservas.ML
{
    public class RecomendacionMLService
    {
        private readonly MLContext _mlContext;
        private readonly ApplicationDbContext _db;
        private ITransformer _modelo;
        private const string MODEL_PATH = "ML/modeloRecomendacion.zip";

        public RecomendacionMLService(ApplicationDbContext db)
        {
            _mlContext = new MLContext(seed: 0);
            _db = db;
        }

        // Entrena el modelo con las reservas actuales en BD
        public void EntrenarModelo()
        {
            // 1) Preparar datos: tomar reservas con su restaurante relacionado
            var data = _db.Reservas
                .Include(r => r.Restaurante)
                .Where(r => r.Restaurante != null && !string.IsNullOrEmpty(r.Restaurante.Tipo))
                .Select(r => new ReservaMLData
                {
                    UserId = r.UserId,
                    Distrito = r.Restaurante.Distrito ?? "",
                    Rating = (float)r.Restaurante.Rating,
                    Tipo = r.Restaurante.Tipo
                })
                .ToList();

            if (data == null || data.Count < 5)
                throw new InvalidOperationException("No hay suficientes datos para entrenar el modelo. Agrega reservas reales para entrenar.");

            var trainingData = _mlContext.Data.LoadFromEnumerable(data);

            // 2) Construir pipeline
            var pipeline = _mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "Label", inputColumnName: nameof(ReservaMLData.Tipo))
                // transformar UserId y Distrito a features
                .Append(_mlContext.Transforms.Text.FeaturizeText(outputColumnName: "UserIdFeats", inputColumnName: nameof(ReservaMLData.UserId)))
                .Append(_mlContext.Transforms.Text.FeaturizeText(outputColumnName: "DistritoFeats", inputColumnName: nameof(ReservaMLData.Distrito)))
                // concatenar features y el rating numérico
                .Append(_mlContext.Transforms.Concatenate("Features", "UserIdFeats", "DistritoFeats", nameof(ReservaMLData.Rating)))
                // Entrenador multiclass
                .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(labelColumnName: "Label", featureColumnName: "Features"))
                .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            // 3) Fit
            _modelo = pipeline.Fit(trainingData);

            // 4) Guardar
            Directory.CreateDirectory(Path.GetDirectoryName(MODEL_PATH) ?? "ML");
            _mlContext.Model.Save(_modelo, trainingData.Schema, MODEL_PATH);
        }

        // Carga el modelo desde disco (si existe)
        private void CargarModeloSiEsNecesario()
        {
            if (_modelo != null) return;
            if (File.Exists(MODEL_PATH))
            {
                _modelo = _mlContext.Model.Load(MODEL_PATH, out var schema);
            }
        }

        // Predice la categoría preferida dado un userId
        public string PredecirTipoFavorito(string userId)
        {
            CargarModeloSiEsNecesario();
            if (_modelo == null)
                throw new InvalidOperationException("El modelo no está entrenado. Llama a EntrenarModelo() primero.");

            var predEngine = _mlContext.Model.CreatePredictionEngine<ReservaMLData, TipoPrediccion>(_modelo);

            // Preparar input: usamos distrito vacío y rating promedio (puedes calcularlo)
            float ratingPromedio = (float)(_db.Reservas
                .Include(r => r.Restaurante)
                .Where(r => r.UserId == userId && r.Restaurante != null)
                .Select(r => r.Restaurante.Rating)
                .DefaultIfEmpty(4.0m)
                .Average());

            var input = new ReservaMLData
            {
                UserId = userId,
                Distrito = "",
                Rating = ratingPromedio
            };

            var pred = predEngine.Predict(input);
            return pred?.TipoPredicho;
        }

        // Recomendar restaurantes (filtra por el tipo predicho y ordena por rating)
        public List<Restaurante> RecomendarRestaurantes(string userId, int cantidad = 5)
        {
            string tipoPredicho;
            try
            {
                tipoPredicho = PredecirTipoFavorito(userId);
            }
            catch
            {
                tipoPredicho = null;
            }

            if (string.IsNullOrEmpty(tipoPredicho))
            {
                // fallback: top global por rating
                return _db.Restaurantes
                    .OrderByDescending(r => r.Rating)
                    .Take(cantidad)
                    .ToList();
            }

            return _db.Restaurantes
                .Where(r => r.Tipo == tipoPredicho)
                .OrderByDescending(r => r.Rating)
                .Take(cantidad)
                .ToList();
        }

        // Método helper: reentrenar y guardar (útil para administración o cron manual)
        public void ReentrenarYGuardar()
        {
            EntrenarModelo();
        }
    }
}
