using System.IO;
using System.Text;
using DiscUtils;
using DiscUtils.Fat;
using DiscUtils.Vfat;
using Xunit;

namespace LibraryTests.Vfat
{
	public class VfatFileSystemTest
	{
		[Fact]
		public void MakeAutounattend()
		{
			var xml = "<xml>Hello World!</xml>";

			using (var s = new MemoryStream())
			{
				using (var f = FatFileSystem.FormatFloppy<VfatFileSystem>(s, FloppyDiskType.HighDensity, null))
				{
					using (var fs = f.OpenFile("autounattend.xml", FileMode.Create))
					using (var sw = new StreamWriter(fs))
					{
						sw.WriteLine(xml);
					}
				}

				s.Seek(0, SeekOrigin.Begin);

				// TODO: For now we use FatFileSystem to read back the file data.
				using (var f = new FatFileSystem(s))
				{
					using (var fs = f.OpenFile("AUTOUN~1.XML", FileMode.Open, FileAccess.Read))
					using (var sr = new StreamReader(fs))
					{
						var data = sr.ReadLine();
						Assert.Equal(data, xml);
					}
				}
			}
		}

		[Fact]
		public void CreateDirectories() {
			string strTestData = "Test text";

			using (Stream stmImage = new MemoryStream()) {
				using (DiscFileSystem fs = FatFileSystem.FormatFloppy<VfatFileSystem> (stmImage, FloppyDiskType.HighDensity, null)) {
					fs.CreateDirectory(@"\TopLevelDirectory");
					fs.CreateDirectory(@"\TopLevelDirectory\1LvlDn");

					using (Stream stmFile = fs.OpenFile(@"\TopLevelDirectory\1LvlDn\ThisIsMyFile.foo", FileMode.Create, FileAccess.Write))
						WriteAsciiText(stmFile, strTestData);
                }

				stmImage.Seek(0, SeekOrigin.Begin);

				using (DiscFileSystem fs = new FatFileSystem(stmImage)) {
					Assert.True(fs.DirectoryExists(@"\TOPLEV~1"));
					Assert.True(fs.DirectoryExists(@"\TOPLEV~1\1LVLDN"));

					using (Stream stmFile = fs.OpenFile(@"\TOPLEV~1\1LVLDN\THISIS~1.FOO", FileMode.Open, FileAccess.Read))
						Assert.Equal(strTestData, ReadAsciiText(stmFile));
                }
            }
        }

		[Fact]
		public void ModifyFile() {
			string strData1 = "Initial file data";
			string strData2 = "Modified file data - size has also changed";
			string strFilePath = @"\My_Test_File-01.T-T";

			using (Stream stmImage = new MemoryStream()) {
				using (DiscFileSystem fs = FatFileSystem.FormatFloppy<VfatFileSystem>(stmImage, FloppyDiskType.HighDensity, null)) {
					using (Stream stmInitialWrite = fs.OpenFile(strFilePath, FileMode.CreateNew, FileAccess.Write))
						WriteAsciiText(stmInitialWrite, strData1);

					using (Stream stmFirstRead = fs.OpenFile(strFilePath, FileMode.Open, FileAccess.Read))
						Assert.Equal(ReadAsciiText(stmFirstRead), strData1);

					using (Stream stmModifyWrite = fs.OpenFile(strFilePath, FileMode.Create, FileAccess.Write))
						WriteAsciiText(stmModifyWrite, strData2);

					using (Stream stmSecondRead = fs.OpenFile(strFilePath, FileMode.Open, FileAccess.Read))
						Assert.Equal(ReadAsciiText(stmSecondRead), strData2);
                }
            }
        }

		[Fact]
		public void SimilarNames() {
			string strData1 = "First file data";
			string strData2 = "Second file data";
			string strFilePath1 = @"\My_Test_File-01.123";
			string strFilePath2 = @"\My_Test_File-02.123";

			using (Stream stmImage = new MemoryStream()) {
				using (DiscFileSystem fs = FatFileSystem.FormatFloppy<VfatFileSystem>(stmImage, FloppyDiskType.HighDensity, null)) {
					// write file 1 and read back
					using (Stream stmFile1Write = fs.OpenFile(strFilePath1, FileMode.CreateNew, FileAccess.Write))
						WriteAsciiText(stmFile1Write, strData1);

					using (Stream stmFile1Read = fs.OpenFile(strFilePath1, FileMode.Open, FileAccess.Read))
						Assert.Equal(ReadAsciiText(stmFile1Read), strData1);

					// write file 2 and read back
					using (Stream stmFile2Write = fs.OpenFile(strFilePath2, FileMode.Create, FileAccess.Write))
						WriteAsciiText(stmFile2Write, strData2);

					using (Stream stmFile2Read = fs.OpenFile(strFilePath2, FileMode.Open, FileAccess.Read))
						Assert.Equal(ReadAsciiText(stmFile2Read), strData2);

					// check that file 1 hasn't changed
					using (Stream stmFile1Read = fs.OpenFile(strFilePath1, FileMode.Open, FileAccess.Read))
						Assert.Equal(ReadAsciiText(stmFile1Read), strData1);
				}
			}
		}

		private void WriteAsciiText(Stream stmFile, string strText) {
			byte[] arrData = Encoding.ASCII.GetBytes(strText);
			stmFile.Write(arrData, 0, arrData.Length);
        }

		private string ReadAsciiText(Stream stmFile) {
			string str = "";
			int nByteVal;

			while ((nByteVal = stmFile.ReadByte()) != -1)
				str += (char)nByteVal;

			return str;
        }
	}
}
