using Microsoft.ML.Data;

namespace appReservas.ML
{
    public class TipoPrediccion
    {
        [ColumnName("PredictedLabel")]
        public string TipoPredicho { get; set; }

        // Opcional: scores por cada clase
        public float[] Score { get; set; }
    }
}
