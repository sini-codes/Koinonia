using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using UnityEngine;

namespace Koinonia
{
    public static class ZipUtils
    {

        public static void Decompress(string id, byte[] gzBytes)
        {
            ZipInputStream s = null;
            try
            {
                using (s = new ZipInputStream(new MemoryStream(gzBytes)))
                {

                    ZipEntry theEntry;
                    while ((theEntry = s.GetNextEntry()) != null)
                    {
                        //Debug.Log("Extracting: " + theEntry.Name);
                        if (theEntry.IsDirectory)
                        {

                            //Console.WriteLine("Creating folder: "+ theEntry.Name);
                            Directory.CreateDirectory(Path.Combine(id, theEntry.Name));
                        } else if(theEntry.IsFile){



                            string fileName = Path.GetFileName(theEntry.Name);
                            //Console.WriteLine("Creating file: " + fileName);

                            // create directory

                            if (fileName != String.Empty)
                            {
                                using (FileStream streamWriter = File.Create(Path.Combine(id, theEntry.Name)))
                                {

                                    int size = 2048;
                                    byte[] data = new byte[2048];
                                    while (true)
                                    {
                                        size = s.Read(data, 0, data.Length);
                                        if (size > 0)
                                        {
                                            streamWriter.Write(data, 0, size);
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

            }
            finally
            {
                if (s != null)
                {
                    s.IsStreamOwner = true; // Makes close also shut the underlying stream
                    s.Close(); // Ensure we release resources
                }
            }
        }

        public static void CopyTo(this Stream input, Stream output)
        {
            byte[] buffer = new byte[16 * 1024]; // Fairly arbitrary size
            int bytesRead;

            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, bytesRead);
            }
        }
    }
}