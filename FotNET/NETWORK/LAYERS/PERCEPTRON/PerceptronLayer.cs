﻿using FotNET.NETWORK.OBJECTS;

namespace FotNET.NETWORK.LAYERS.PERCEPTRON {
    public class PerceptronLayer : ILayer {

        public PerceptronLayer(int size, int nextSize) {
            Neurons      = new double[size];
            NeuronsError = new double[size];
            Bias         = new double[size];

            Weights = new Matrix(nextSize, size);
            Weights.HeInitialization();

            for (var i = 0; i < size; i++)
                Bias[i] = .001d;
            
            _isEndLayer = false;
        }

        public PerceptronLayer(int size) {
            Neurons      = new double[size];
            NeuronsError = new double[size];
            Bias         = new double[size];

            Weights = new Matrix(size, size);
            for (var i = 0; i < size; i++)
                Weights.Body[i, i] = 1;

            _isEndLayer = true;
        }

        private readonly bool _isEndLayer;
        private double[] Neurons { get; set; }
        private double[] Bias { get; }
        private double[] NeuronsError { get; set; }
        private Matrix Weights { get; }

        public Tensor GetValues() => new Vector(Neurons).AsTensor(1, Neurons.Length, 1);

        public Tensor GetNextLayer(Tensor tensor) {
            Neurons = tensor.Flatten().ToArray();
            var nextLayer = new Vector(Weights * Neurons) + new Vector(Bias);
            return new Vector(nextLayer).AsTensor(1, nextLayer.Length, 1);
        }

        public Tensor BackPropagate(Tensor error, double learningRate) {
            var previousError = error.Flatten().ToArray();
            if (_isEndLayer) return new Vector(previousError).AsTensor(1, previousError.Length, 1);
            
            NeuronsError = Weights.Transpose() * previousError;
            for (var j = 0; j < Weights.Body.GetLength(0); ++j)
                for (var k = 0; k < Weights.Body.GetLength(1); ++k)
                    Weights.Body[j, k] -= Neurons[k] * previousError[j] * learningRate;

            for (var j = 0; j < Weights.Body.GetLength(1); j++)
                Bias[j] -= NeuronsError[j] * learningRate;

            return new Vector(NeuronsError).AsTensor(1, NeuronsError.Length, 1);
        }

        public string GetData() {
            var temp = "";
            temp += Weights.GetValues();
            return Bias.Aggregate(temp, (current, bias) => current + bias + " ");
        }

        public string LoadData(string data) {
            var position = 0;
            var dataNumbers = data.Split(" ");

            for (var i = 0; i < Weights.Body.GetLength(0); i++)
                for (var j = 0; j < Weights.Body.GetLength(1); j++)
                    Weights.Body[i, j] = double.Parse(dataNumbers[position++]);

            for (var j = 0; j < Bias.Length; j++)
                Bias[j] = double.Parse(dataNumbers[position++]);

            return string.Join(" ", dataNumbers.Skip(position).Select(p => p.ToString()).ToArray());
        }
    }
}