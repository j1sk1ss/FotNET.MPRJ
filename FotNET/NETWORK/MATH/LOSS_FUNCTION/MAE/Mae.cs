﻿using FotNET.NETWORK.MATH.OBJECTS;

namespace FotNET.NETWORK.MATH.LOSS_FUNCTION.MAE;

/// <summary>
/// Mean absolute error (MAE)
/// </summary>
public class Mae : LossFunction {
    protected override double GetValue(Tensor expected, Tensor predicted, int channel, int x, int y) => 
        Math.Abs(expected.Channels[channel].Body[x, y] - predicted.Channels[channel].Body[x, y]);
    
    protected override double GetValueForError(Tensor expected, Tensor predicted, int channel, int x, int y) => 
        predicted.Channels[channel].Body[x, y] - expected.Channels[channel].Body[x, y];
}