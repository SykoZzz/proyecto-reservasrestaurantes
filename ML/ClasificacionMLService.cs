using Microsoft.ML;
using Microsoft.ML.Data;
using appReservas.Models;

namespace appReservas.ML
{
    public class ClasificacionMLService
    {
        private readonly string _modelPath = Path.Combine(Directory.GetCurrentDirectory(), "MLModel_Clasificacion.zip");
        private readonly MLContext _mlContext;

        public ClasificacionMLService()
        {
            _mlContext = new MLContext();
        }

        // Entrena el modelo usando los restaurantes existentes
        public void EntrenarModelo(List<Restaurante> restaurantes)
        {
            var data = restaurantes.Select(r => new ClasificacionInput
            {
                PrecioPromedio = (float)r.PrecioPromedio,
                Rating = (float)r.Rating,
                Tipo = r.Tipo ?? "Desconocido",
                Distrito = r.Distrito ?? "Desconocido",
                CategoriaPrecio = r.PrecioPromedio switch
                {
                    < 40 => "Económico",
                    >= 40 and < 80 => "Medio",
                    _ => "Premium"
                }
            }).ToList();

            var trainingData = _mlContext.Data.LoadFromEnumerable(data);

            // Definir el pipeline
            var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(ClasificacionInput.CategoriaPrecio))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding("TipoEncoded", nameof(ClasificacionInput.Tipo)))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding("DistritoEncoded", nameof(ClasificacionInput.Distrito)))
                .Append(_mlContext.Transforms.Concatenate("Features", "PrecioPromedio", "Rating", "TipoEncoded", "DistritoEncoded"))
                .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
                .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            // Entrenar
            var model = pipeline.Fit(trainingData);

            // Guardar modelo
            _mlContext.Model.Save(model, trainingData.Schema, _modelPath);
        }

        // Predicción para un restaurante
        public string PredecirCategoria(Restaurante r)
        {
            if (!File.Exists(_modelPath))
                throw new FileNotFoundException("Modelo no encontrado. Entrénalo primero.");

            var model = _mlContext.Model.Load(_modelPath, out _);
            var predEngine = _mlContext.Model.CreatePredictionEngine<ClasificacionInput, ClasificacionOutput>(model);

            var input = new ClasificacionInput
            {
                PrecioPromedio = (float)r.PrecioPromedio,
                Rating = (float)r.Rating,
                Tipo = r.Tipo ?? "Desconocido",
                Distrito = r.Distrito ?? "Desconocido"
            };

            var prediction = predEngine.Predict(input);
            return prediction.PredictedLabel;
        }
    }
}
