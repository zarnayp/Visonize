using NUnit.Framework;

namespace DupploPulse.UsImaging.Infrastructure.Dicom.Tests
{
    public class DicomFileUtilsTest
    {
        [Test]
        public void DicomFileUtilsTest_GetCartezian3DImageDataInfo_Success()
        {
            DicomFileUtils dicomFileUtils = new DicomFileUtils();

            var path = Path.Combine(AppContext.BaseDirectory, "assets", "cartesian.ima");

            var info = dicomFileUtils.GetCartezian3DImageDataInfo(path);

            Assert.AreEqual(279, info.width);
            Assert.AreEqual(280, info.height);
            Assert.AreEqual(280, info.depth);

            Assert.AreEqual(279*280*280, info.firstFrameImageData.Length);
        }
    }
}
