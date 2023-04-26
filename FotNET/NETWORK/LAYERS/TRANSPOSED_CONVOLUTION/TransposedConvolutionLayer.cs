using FotNET.NETWORK.LAYERS.CONVOLUTION.SCRIPTS;
using FotNET.NETWORK.LAYERS.TRANSPOSED_CONVOLUTION.SCRIPTS;
using FotNET.NETWORK.MATH.Initialization;
using FotNET.NETWORK.MATH.OBJECTS;

namespace FotNET.NETWORK.LAYERS.TRANSPOSED_CONVOLUTION;

public class TransposedConvolutionLayer : ILayer {
    /// <summary> Layer that perform tensor deconvolution by filters and biases. </summary>
    /// <param name="filters"> Count of filters on layer. </param>
    /// <param name="filterWeight"> Weight of filters on layer. </param>
    /// <param name="filterHeight"> Height of filters on layer. </param>
    /// <param name="filterDepth"> Depth of filters on layer. </param>
    /// <param name="weightsInitialization"> Type of weights initialization of filters on layer. </param>
    /// <param name="stride"> Stride of deconvolution. </param>
    public TransposedConvolutionLayer(int filters, int filterWeight, int filterHeight, int filterDepth,
        IWeightsInitialization weightsInitialization, int stride) {
        Filters = new Filter[filters];
            
        for (var j = 0; j < filters; j++) {
            Filters[j] = new Filter(new List<Matrix>()) {
                Bias = .001d
            };

            for (var i = 0; i < filterDepth; i++)
                Filters[j].Channels.Add(new Matrix(
                    new double[filterWeight, filterHeight]));
        }

        foreach (var filter in Filters)
            for (var i = 0; i < filter.Channels.Count; i++)
                filter.Channels[i] = weightsInitialization.Initialize(filter.Channels[i]);

        _stride = stride;
        Input   = new Tensor(new Matrix(0, 0));
    }
    
    private readonly int _stride;
    
    private Filter[] Filters { get; }
    private Tensor Input { get; set; }
    
    private static Filter[] FlipFilters(Filter[] filters) {
        for (var i = 0; i < filters.Length; i++)
            filters[i] = filters[i].Flip().AsFilter();

        return filters;
    }
    
    private static Filter[] GetFiltersWithoutBiases(Filter[] filters) {
        for (var i = 0; i < filters.Length; i++)
            filters[i] = new Filter(filters[i].Channels);

        return filters;
    }
    
    public Tensor GetNextLayer(Tensor tensor) {
        Input = new Tensor(new List<Matrix>(tensor.Channels));
        return TransposedConvolution.GetTransposedConvolution(tensor, Filters, _stride);
    }

    public Tensor BackPropagate(Tensor error, double learningRate, bool backPropagate) {
        var inputTensor = Input;
        var extendedError = error.GetSameChannels(inputTensor);
        
        var originalFilters = new Filter[Filters.Length];
        for (var i = 0; i < Filters.Length; i++)
            originalFilters[i] = new Filter(new List<Matrix>(Filters[i].Channels));

        if (backPropagate)
            Parallel.For(0, Filters.Length, filter => {
                for (var channel = 0; channel < Filters[filter].Channels.Count; channel++) {
                    Filters[filter].Channels[channel] -= Convolution.GetConvolution(
                        extendedError.Channels[filter], Input.Channels[filter],
                        _stride, Filters[filter].Bias) * learningRate;
                }

                Filters[filter].Bias -= error.Channels[filter].Sum() * learningRate;
            });
        
        return Convolution.GetConvolution(extendedError, 
            FlipFilters(GetFiltersWithoutBiases(originalFilters)), _stride);
    }

    public Tensor GetValues() => Input;

    public string GetData() {
        var temp = "";
            
        foreach (var filter in Filters) {
            temp = filter.Channels.Aggregate(temp, (current, channel) => current + channel.GetValues());
            temp += filter.Bias + " ";
        }
            
        return temp;
    }

    public string LoadData(string data) {
        var position = 0;
        var dataNumbers = data.Split(" ",  StringSplitOptions.RemoveEmptyEntries);

        foreach (var filter in Filters) {
            foreach (var channel in filter.Channels)
                for (var x = 0; x < channel.Rows; x++)
                for (var y = 0; y < channel.Columns; y++)
                    channel.Body[x, y] = double.Parse(dataNumbers[position++]);

            filter.Bias = double.Parse(dataNumbers[position++]);
        }

        return string.Join(" ", dataNumbers.Skip(position).Select(p => p.ToString()).ToArray());
    }
}