using Microsoft.ML.Data;
using  appReservas.Models;
namespace appReservas.ML;
public class ClasificacionOutput
{
    public string PredictedLabel { get; set; }
    public float[] Score { get; set; }
}
