using System.Diagnostics;

namespace Visonize.UsImaging.Infrastructure.Stream.Tests;

public class RgbExtractTest
{
    [SetUp]
    public void Setup()
    {
    }

    private bool ValidateComponents(byte[] outRed, byte[] outGreen, byte[] outBlue, byte[] inputRgba)
    {
        int pixelCount = inputRgba.Length / 4;

        if (outRed.Length != pixelCount || outGreen.Length != pixelCount || outBlue.Length != pixelCount)
        {
            Assert.Fail("Array sizes do not match the expected pixel count.");
            return false;
        }

        for (int i = 0; i < pixelCount; i++)
        {
            if (outRed[i] != inputRgba[i * 4] || 
                outGreen[i] != inputRgba[i * 4 + 1] || 
                outBlue[i] != inputRgba[i * 4 + 2])
            {
                Assert.Fail($"Mismatch found at pixel {i}");
                return false;
            }
        }

        return true;
    }
    
    [Test]
    public void RgbExtractTest_ExtractWithEightThreads()
    {
        int size = 1024 * 720;
        
        var inputRgba = new byte[size*4];
        var outputRed = new byte[size];
        var outputGreen = new byte[size];
        var outputBlue = new byte[size];
        
        Random random = new Random();
        random.NextBytes(inputRgba); // Fill array with random values
        
        var extract = new RgbExtract( size, 8);
        
        var sw = Stopwatch.StartNew();
        
        extract.ExtractRGBA(outputRed, outputGreen, outputBlue, inputRgba);
       
        sw.Stop();
        
        extract.Dispose();
        
        Assert.IsTrue(ValidateComponents(outputRed, outputGreen, outputBlue, inputRgba));
        
        Assert.Pass($"Extract done in {sw.ElapsedMilliseconds} ms");
    }
    
    [Test]
    public void RgbExtractTest_ExtractWithOneThreads()
    {
        int size = 1024 * 720;
        
        var inputRgba = new byte[size*4];
        var outputRed = new byte[size];
        var outputGreen = new byte[size];
        var outputBlue = new byte[size];
        
        Random random = new Random();
        random.NextBytes(inputRgba); // Fill array with random values
        
        var extract = new RgbExtract( size, 1);
        
        var sw = Stopwatch.StartNew();
        
        extract.ExtractRGBA(outputRed, outputGreen, outputBlue, inputRgba);
       
        sw.Stop();
        
        extract.Dispose();
        
        Assert.IsTrue(ValidateComponents(outputRed, outputGreen, outputBlue, inputRgba));
        
        Assert.Pass($"Extract done in {sw.ElapsedMilliseconds} ms");
    }
    
    [Test]
    public void RgbExtractTest_ExtractWithNineThreads()
    {
        int size = 1024 * 720;
        
        var inputRgba = new byte[size*4];
        var outputRed = new byte[size];
        var outputGreen = new byte[size];
        var outputBlue = new byte[size];
        
        Random random = new Random();
        random.NextBytes(inputRgba); // Fill array with random values
        
        var extract = new RgbExtract( size, 9);
        
        var sw = Stopwatch.StartNew();
        
        extract.ExtractRGBA(outputRed, outputGreen, outputBlue, inputRgba);
       
        sw.Stop();
        
        extract.Dispose();
        
        Assert.IsTrue(ValidateComponents(outputRed, outputGreen, outputBlue, inputRgba));
        
        Assert.Pass($"Extract done in {sw.ElapsedMilliseconds} ms");
    }
}