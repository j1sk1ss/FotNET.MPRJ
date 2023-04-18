﻿using System.Drawing;
using System.Drawing.Imaging;
using FotNET.DATA.IMAGE;
using FotNET.NETWORK;
using FotNET.NETWORK.MATH.LOSS_FUNCTION.RATING.MAE;
using FotNET.NETWORK.MATH.LOSS_FUNCTION.RATING.MSE;
using FotNET.NETWORK.MATH.OBJECTS;

namespace FotNET.SCRIPTS.GENERATIVE_ADVERSARIAL_NETWORK;


public class GaNetwork {
    /// <summary>
    /// Generative Adversarial model 
    /// </summary>
    /// <param name="generator"> Generator model </param>
    /// <param name="discriminator"> Discriminator model </param>
    public GaNetwork(Network generator, Network discriminator) {
        Generator = generator;
        Discriminator = discriminator;
    }

    private Network Generator { get; }
    private Network Discriminator { get; }

    public Network GetGenerator() => Generator;
    public Network GetDiscriminator() => Discriminator;


    private List<Tensor> GenerateFake(int count) {
        var fake = new List<Tensor>();
        for (var i = 0; i < count; i++) 
            fake.Add(Generator.ForwardFeed(null));
        
        return fake;
    }

    /// <summary>
    /// Generate real data set
    /// </summary>
    /// <param name="directoryPath"> Path for directory with images </param>
    /// <param name="resizeX"> End size of bitmap </param>
    /// <param name="resizeY"> End size of bitmap </param>
    /// <returns></returns>
    public static List<Tensor> LoadReal(string directoryPath, int resizeX, int resizeY) {
        var files = Directory.GetFiles(directoryPath);
        return files.Select(file => Parser.ImageToTensor
            (new Bitmap((Bitmap)Image.FromFile(file), new Size(resizeX, resizeY)))).ToList();
    }
    
    /// <summary>
    /// Discriminator fitting
    /// </summary>
    /// <param name="epochs"> Epochs count </param>
    /// <param name="realDataSet"> Real data set </param>
    /// <param name="learningRate"> Learning rate </param>
    public void DiscriminatorFitting(int epochs, List<Tensor> realDataSet, double learningRate) {
        for (var j = 0; j < epochs; j++) {
            var fakeDataSet = GenerateFake(realDataSet.Count);
            for (var i = 0; i < realDataSet.Count; i++)
                switch (new Random().Next() % 100 > 50) {
                    case true: // load real 1
                        if (Math.Abs(Discriminator.ForwardFeed(realDataSet[i], AnswerType.Class) - 1) > .1)
                            Discriminator.BackPropagation(1, 1, new Mae(), learningRate, true);
                        break;
                    case false: // load fake 0
                        if (Discriminator.ForwardFeed(fakeDataSet[i], AnswerType.Class) > 0.01d)
                            Discriminator.BackPropagation(0, 1, new Mae(), learningRate, true);
                        break;
                }
        }
    }

    /// <summary>
    /// Generator fitting
    /// </summary>
    /// <param name="epochs"> Epochs count </param>
    /// <param name="learningRate"> Learning rate </param>
    public void GeneratorFitting(int epochs, double learningRate) {
        for (var i = 0; i < epochs; i++) {
            var generated = Generator.ForwardFeed(null);
            if (i % 100 == 0)
                Parser.TensorToImage(generated).Save(@$"C://Users//j1sk1ss//Desktop//RCNN_TEST//answers//{Guid.NewGuid()}.png", ImageFormat.Png);
            var answer = Discriminator.ForwardFeed(generated, AnswerType.Class);
            if (Math.Abs(answer - 1) > .1) 
                Generator.BackPropagation(Discriminator.BackPropagation(1,1, 
                    new Mae(), learningRate, false), learningRate, true);
        }
    }

    /// <summary>
    /// Generate bitmap by generator model
    /// </summary>
    /// <returns> Generated bitmap </returns>
    public Bitmap GenerateBitmap() =>
        Parser.TensorToImage(Generator.ForwardFeed(null!));

    /// <summary>
    /// Generate tensor by generator model
    /// </summary>
    /// <returns> Generated tensor </returns>
    public Tensor GenerateTensor() {
        return Generator.ForwardFeed(null!);
    }
}