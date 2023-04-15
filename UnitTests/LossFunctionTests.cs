using FotNET.NETWORK.MATH.LOSS_FUNCTION.ONE_BY_ONE;
using FotNET.NETWORK.MATH.LOSS_FUNCTION.VALUE_BY_VALUE;
using FotNET.NETWORK.MATH.OBJECTS;

namespace UnitTests;

public class LossFunctionTests
{
    [Test]
    public void OneByOne() {
        var predicted = new Tensor(new Matrix(new[] { .16d, .1d, .07d, .9d, .13d, .129d }));
        var expected = new Tensor(new Matrix(new[] { .12d, .44d, .76d, .11d, .4d, .13d }));
        
        Console.WriteLine(new Vector(new OneByOne().GetErrorTensor(predicted, expected).Flatten().ToArray()).Print());
    }

    [Test]
    public void ValueByValue() {
        var predicted = new Tensor(new Matrix(new[] { .16d, .1d, .07d, .9d, .13d, .129d }));
        var expected = new Tensor(new Matrix(new[] { .12d, .44d, .76d, .11d, .4d, .13d }));
        
        Console.WriteLine(new Vector(new ValueByValue().GetErrorTensor(predicted, expected).Flatten().ToArray()).Print());
    }
}