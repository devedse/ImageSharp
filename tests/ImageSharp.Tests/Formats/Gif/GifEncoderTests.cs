﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.IO;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.MetaData;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Quantization;
using Xunit;
// ReSharper disable InconsistentNaming

namespace SixLabors.ImageSharp.Tests
{
    public class GifEncoderTests
    {
        private const PixelTypes TestPixelTypes = PixelTypes.Rgba32 | PixelTypes.RgbaVector | PixelTypes.Argb32;

        [Theory]
        [WithTestPatternImages(100, 100, TestPixelTypes)]
        public void EncodeGeneratedPatterns<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> image = provider.GetImage())
            {
                provider.Utility.SaveTestOutputFile(image, "gif", new GifEncoder() { Quantizer = new PaletteQuantizer() });
            }
        }

        [Fact]
        public void Encode_IgnoreMetadataIsFalse_CommentsAreWritten()
        {
            var options = new GifEncoder()
            {
                IgnoreMetadata = false
            };

            var testFile = TestFile.Create(TestImages.Gif.Rings);

            using (Image<Rgba32> input = testFile.CreateImage())
            {
                using (var memStream = new MemoryStream())
                {
                    input.Save(memStream, options);

                    memStream.Position = 0;
                    using (var output = Image.Load<Rgba32>(memStream))
                    {
                        Assert.Equal(1, output.MetaData.Properties.Count);
                        Assert.Equal("Comments", output.MetaData.Properties[0].Name);
                        Assert.Equal("ImageSharp", output.MetaData.Properties[0].Value);
                    }
                }
            }
        }

        [Fact]
        public void Encode_IgnoreMetadataIsTrue_CommentsAreNotWritten()
        {
            var options = new GifEncoder()
            {
                IgnoreMetadata = true
            };

            var testFile = TestFile.Create(TestImages.Gif.Rings);

            using (Image<Rgba32> input = testFile.CreateImage())
            {
                using (var memStream = new MemoryStream())
                {
                    input.SaveAsGif(memStream, options);

                    memStream.Position = 0;
                    using (var output = Image.Load<Rgba32>(memStream))
                    {
                        Assert.Equal(0, output.MetaData.Properties.Count);
                    }
                }
            }
        }

        [Fact]
        public void Encode_WhenCommentIsTooLong_CommentIsTrimmed()
        {
            using (var input = new Image<Rgba32>(1, 1))
            {
                string comments = new string('c', 256);
                input.MetaData.Properties.Add(new ImageProperty("Comments", comments));

                using (var memStream = new MemoryStream())
                {
                    input.Save(memStream, new GifEncoder());

                    memStream.Position = 0;
                    using (var output = Image.Load<Rgba32>(memStream))
                    {
                        Assert.Equal(1, output.MetaData.Properties.Count);
                        Assert.Equal("Comments", output.MetaData.Properties[0].Name);
                        Assert.Equal(255, output.MetaData.Properties[0].Value.Length);
                    }
                }
            }
        }
    }
}
