﻿using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using System.Drawing;
using ManicDigger.Collisions;

namespace ManicDigger
{
    public class TerrainChunkRenderer
    {
        [Inject]
        public ITerrainInfo mapstorage { get; set; }
        [Inject]
        public IGameData data { get; set; }
        [Inject]
        public Config3d config3d { get; set; }
        [Inject]
        public ITerrainRenderer terrainrenderer { get; set; }//textures
        public bool DONOTDRAWEDGES = true;
        public int chunksize = 16; //16x16
        public int texturesPacked = 16;
        public float BlockShadow = 0.6f;
        public bool ENABLE_ATLAS1D = true;
        int maxblocktypes = 256;
        public IEnumerable<VerticesIndicesToLoad> MakeChunk(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0) { yield break; }
            if (x >= mapstorage.MapSizeX / chunksize
                || y >= mapstorage.MapSizeY / chunksize
                || z >= mapstorage.MapSizeZ / chunksize) { yield break; }
            if (ENABLE_ATLAS1D)
            {
                toreturnatlas1d = new VerticesIndices[maxblocktypes / terrainrenderer.terrainTexturesPerAtlas];
                toreturnatlas1dtransparent = new VerticesIndices[maxblocktypes / terrainrenderer.terrainTexturesPerAtlas];
                for (int i = 0; i < toreturnatlas1d.Length; i++)
                {
                    toreturnatlas1d[i] = new VerticesIndices();
                    toreturnatlas1dtransparent[i] = new VerticesIndices();
                }
            }
            //else
            {
                toreturnmain = new VerticesIndices();
                toreturntransparent = new VerticesIndices();
            }
            byte[, ,] currentChunk = new byte[chunksize + 2, chunksize + 2, chunksize + 2];
            for (int xx = 0; xx < chunksize + 2; xx++)
            {
                for (int yy = 0; yy < chunksize + 2; yy++)
                {
                    for (int zz = 0; zz < chunksize + 2; zz++)
                    {
                        int xxx = x * chunksize + xx - 1;
                        int yyy = y * chunksize + yy - 1;
                        int zzz = z * chunksize + zz - 1;
                        if (!IsValidPos(xxx, yyy, zzz))
                        {
                            continue;
                        }
                        currentChunk[xx, yy, zz] = (byte)mapstorage.GetTerrainBlock(xxx, yyy, zzz);
                    }
                }
            }
            currentChunkShadows = new float[chunksize + 2, chunksize + 2, chunksize + 2];
            currentChunkIgnore = new bool[chunksize, chunksize, chunksize, 6];
            for (int xx = 0; xx < chunksize + 2; xx++)
            {
                for (int yy = 0; yy < chunksize + 2; yy++)
                {
                    for (int zz = 0; zz < chunksize + 2; zz++)
                    {
                        currentChunkShadows[xx, yy, zz] = float.NaN;
                    }
                }
            }

            for (int xx = 0; xx < chunksize; xx++)
            {
                for (int yy = 0; yy < chunksize; yy++)
                {
                    for (int zz = 0; zz < chunksize; zz++)
                    {
                        int xxx = x * chunksize + xx;
                        int yyy = y * chunksize + yy;
                        int zzz = z * chunksize + zz;
                        BlockPolygons(xxx, yyy, zzz, currentChunk);
                    }
                }
            }
            if (ENABLE_ATLAS1D)
            {
                for (int i = 0; i < toreturnatlas1d.Length; i++)
                {
                    if (toreturnatlas1d[i].indices.Count > 0)
                    {
                        yield return new VerticesIndicesToLoad()
                        {
                            indices = toreturnatlas1d[i].indices.ToArray(),
                            vertices = toreturnatlas1d[i].vertices.ToArray(),
                            position =
                                new Vector3(x * chunksize, y * chunksize, z * chunksize),
                            texture = terrainrenderer.terrainTextures1d[i % terrainrenderer.terrainTexturesPerAtlas],
                        };
                    }
                }
                for (int i = 0; i < toreturnatlas1dtransparent.Length; i++)
                {
                    if (toreturnatlas1dtransparent[i].indices.Count > 0)
                    {
                        yield return new VerticesIndicesToLoad()
                        {
                            indices = toreturnatlas1dtransparent[i].indices.ToArray(),
                            vertices = toreturnatlas1dtransparent[i].vertices.ToArray(),
                            position =
                                new Vector3(x * chunksize, y * chunksize, z * chunksize),
                            texture = terrainrenderer.terrainTextures1d[i % terrainrenderer.terrainTexturesPerAtlas],
                            transparent = true,
                        };
                    }
                }
            }
            //else
            {
                if (toreturnmain.indices.Count > 0)
                {
                    yield return new VerticesIndicesToLoad()
                    {
                        indices = toreturnmain.indices.ToArray(),
                        vertices = toreturnmain.vertices.ToArray(),
                        position =
                            new Vector3(x * chunksize, y * chunksize, z * chunksize),
                        texture = terrainrenderer.terrainTexture,
                    };
                }
                if (toreturntransparent.indices.Count > 0)
                {
                    yield return new VerticesIndicesToLoad()
                    {
                        indices = toreturntransparent.indices.ToArray(),
                        vertices = toreturntransparent.vertices.ToArray(),
                        position =
                            new Vector3(x * chunksize, y * chunksize, z * chunksize),
                        transparent = true,
                        texture = terrainrenderer.terrainTexture,
                    };
                }
            }
        }
        VerticesIndices toreturnmain;
        VerticesIndices toreturntransparent;
        VerticesIndices[] toreturnatlas1d;
        VerticesIndices[] toreturnatlas1dtransparent;
        class VerticesIndices
        {
            public List<ushort> indices = new List<ushort>();
            public List<VertexPositionTexture> vertices = new List<VertexPositionTexture>();
        }
        private bool IsValidPos(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0)
            {
                return false;
            }
            if (x >= mapstorage.MapSizeX || y >= mapstorage.MapSizeY || z >= mapstorage.MapSizeZ)
            {
                return false;
            }
            return true;
        }
        float[, ,] currentChunkShadows;
        bool[, , ,] currentChunkIgnore;
        private void BlockPolygons(int x, int y, int z, byte[, ,] currentChunk)
        {
            int xx = x % chunksize + 1;
            int yy = y % chunksize + 1;
            int zz = z % chunksize + 1;
            var tt = currentChunk[xx, yy, zz];
            if (!data.IsValidTileType(tt))
            {
                return;
            }
            bool drawtop = IsTileEmptyForDrawingOrTransparent(xx, yy, zz + 1, tt, currentChunk) && (!currentChunkIgnore[xx - 1, yy - 1, zz - 1, (int)TileSide.Top]);
            bool drawbottom = IsTileEmptyForDrawingOrTransparent(xx, yy, zz - 1, tt, currentChunk) && (!currentChunkIgnore[xx - 1, yy - 1, zz - 1, (int)TileSide.Bottom]);
            bool drawfront = IsTileEmptyForDrawingOrTransparent(xx - 1, yy, zz, tt, currentChunk) && (!currentChunkIgnore[xx - 1, yy - 1, zz - 1, (int)TileSide.Front]);
            bool drawback = IsTileEmptyForDrawingOrTransparent(xx + 1, yy, zz, tt, currentChunk) && (!currentChunkIgnore[xx - 1, yy - 1, zz - 1, (int)TileSide.Back]);
            bool drawleft = IsTileEmptyForDrawingOrTransparent(xx, yy - 1, zz, tt, currentChunk) && (!currentChunkIgnore[xx - 1, yy - 1, zz - 1, (int)TileSide.Left]);
            bool drawright = IsTileEmptyForDrawingOrTransparent(xx, yy + 1, zz, tt, currentChunk) && (!currentChunkIgnore[xx - 1, yy - 1, zz - 1, (int)TileSide.Right]);
            int tiletype = tt;
            if (!(drawtop || drawbottom || drawfront || drawback || drawleft || drawright))
            {
                return;
            }
            FastColor color = mapstorage.GetTerrainBlockColor(x, y, z);
            FastColor colorShadowSide = new FastColor(color.A,
                (int)(color.R * BlockShadow),
                (int)(color.G * BlockShadow),
                (int)(color.B * BlockShadow));
            if (DONOTDRAWEDGES)
            {
                //if the game is fillrate limited, then this makes it much faster.
                //(39fps vs vsync 75fps)
                //bbb.
                if (z == 0) { drawbottom = false; }
                if (x == 0) { drawfront = false; }
                if (x == 256 - 1) { drawback = false; }
                if (y == 0) { drawleft = false; }
                if (y == 256 - 1) { drawright = false; }
            }
            float flowerfix = 0;
            if (data.IsBlockFlower(tiletype))
            {
                drawtop = false;
                drawbottom = false;
                flowerfix = 0.5f;
            }
            float blockheight = 1;
            if (tt == data.TileIdSingleStairs)
            {
                blockheight = 0.5f;
            }
            
            FastColor curcolor = color;
            float blockheight00 = blockheight;
            float blockheight01 = blockheight;
            float blockheight10 = blockheight;
            float blockheight11 = blockheight;
            //top
            if (drawtop)
            {
                curcolor = color;
                float shadowratio = GetShadowRatio(xx, yy, zz + 1, x, y, z + 1);
                if (shadowratio != 1)
                {
                    curcolor = new FastColor(color.A,
                        (int)(color.R * shadowratio),
                        (int)(color.G * shadowratio),
                        (int)(color.B * shadowratio));
                }
                int sidetexture = data.GetTileTextureId(tiletype, TileSide.Top);
                int tilecount = GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratio, TileSide.Top);
                VerticesIndices toreturn = GetToReturn(tt, sidetexture);
                RectangleF texrec = TextureAtlas.TextureCoords1d(sidetexture, terrainrenderer.terrainTexturesPerAtlas, tilecount);
                short lastelement = (short)toreturn.vertices.Count;
                toreturn.vertices.Add(new VertexPositionTexture(x + 0.0f, z + blockheight00, y + 0.0f, texrec.Left, texrec.Top, curcolor));
                toreturn.vertices.Add(new VertexPositionTexture(x + 0.0f, z + blockheight01, y + 1.0f, texrec.Left, texrec.Bottom, curcolor));
                toreturn.vertices.Add(new VertexPositionTexture(x + 1.0f * tilecount, z + blockheight10, y + 0.0f, texrec.Right, texrec.Top, curcolor));
                toreturn.vertices.Add(new VertexPositionTexture(x + 1.0f * tilecount, z + blockheight11, y + 1.0f, texrec.Right, texrec.Bottom, curcolor));
                toreturn.indices.Add((ushort)(lastelement + 0));
                toreturn.indices.Add((ushort)(lastelement + 1));
                toreturn.indices.Add((ushort)(lastelement + 2));
                toreturn.indices.Add((ushort)(lastelement + 1));
                toreturn.indices.Add((ushort)(lastelement + 3));
                toreturn.indices.Add((ushort)(lastelement + 2));
            }
            //bottom - same as top, but z is 1 less.
            if (drawbottom)
            {
                curcolor = colorShadowSide;
                float shadowratio = GetShadowRatio(xx, yy, zz - 1, x, y, z - 1);
                if (shadowratio != 1)
                {
                    curcolor = new FastColor(color.A,
                        (int)(Math.Min(curcolor.R, color.R * shadowratio)),
                        (int)(Math.Min(curcolor.G, color.G * shadowratio)),
                        (int)(Math.Min(curcolor.B, color.B * shadowratio)));
                }
                int sidetexture = data.GetTileTextureId(tiletype, TileSide.Bottom);
                int tilecount = GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratio, TileSide.Bottom);
                VerticesIndices toreturn = GetToReturn(tt, sidetexture);
                RectangleF texrec = TextureAtlas.TextureCoords1d(sidetexture, terrainrenderer.terrainTexturesPerAtlas, tilecount);
                short lastelement = (short)toreturn.vertices.Count;
                toreturn.vertices.Add(new VertexPositionTexture(x + 0.0f, z, y + 0.0f, texrec.Left, texrec.Top, curcolor));
                toreturn.vertices.Add(new VertexPositionTexture(x + 0.0f, z, y + 1.0f, texrec.Left, texrec.Bottom, curcolor));
                toreturn.vertices.Add(new VertexPositionTexture(x + 1.0f * tilecount, z, y + 0.0f, texrec.Right, texrec.Top, curcolor));
                toreturn.vertices.Add(new VertexPositionTexture(x + 1.0f * tilecount, z, y + 1.0f, texrec.Right, texrec.Bottom, curcolor));
                toreturn.indices.Add((ushort)(lastelement + 1));
                toreturn.indices.Add((ushort)(lastelement + 0));
                toreturn.indices.Add((ushort)(lastelement + 2));
                toreturn.indices.Add((ushort)(lastelement + 3));
                toreturn.indices.Add((ushort)(lastelement + 1));
                toreturn.indices.Add((ushort)(lastelement + 2));
            }
            //front
            if (drawfront)
            {
                curcolor = color;
                float shadowratio = GetShadowRatio(xx - 1, yy, zz, x - 1, y, z);
                if (shadowratio != 1)
                {
                    curcolor = new FastColor(color.A,
                        (int)(color.R * shadowratio),
                        (int)(color.G * shadowratio),
                        (int)(color.B * shadowratio));
                }
                int sidetexture = data.GetTileTextureId(tiletype, TileSide.Front);
                int tilecount = GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratio, TileSide.Front);
                VerticesIndices toreturn = GetToReturn(tt, sidetexture);
                RectangleF texrec = TextureAtlas.TextureCoords1d(sidetexture, terrainrenderer.terrainTexturesPerAtlas, tilecount);
                short lastelement = (short)toreturn.vertices.Count;
                toreturn.vertices.Add(new VertexPositionTexture(x + 0 + flowerfix, z + 0, y + 0, texrec.Left, texrec.Bottom, curcolor));
                toreturn.vertices.Add(new VertexPositionTexture(x + 0 + flowerfix, z + 0, y + 1 * tilecount, texrec.Right, texrec.Bottom, curcolor));
                toreturn.vertices.Add(new VertexPositionTexture(x + 0 + flowerfix, z + blockheight00, y + 0, texrec.Left, texrec.Top, curcolor));
                toreturn.vertices.Add(new VertexPositionTexture(x + 0 + flowerfix, z + blockheight01, y + 1 * tilecount, texrec.Right, texrec.Top, curcolor));
                toreturn.indices.Add((ushort)(lastelement + 0));
                toreturn.indices.Add((ushort)(lastelement + 1));
                toreturn.indices.Add((ushort)(lastelement + 2));
                toreturn.indices.Add((ushort)(lastelement + 1));
                toreturn.indices.Add((ushort)(lastelement + 3));
                toreturn.indices.Add((ushort)(lastelement + 2));
            }
            //back - same as front, but x is 1 greater.
            if (drawback)
            {
                curcolor = color;
                float shadowratio = GetShadowRatio(xx + 1, yy, zz, x + 1, y, z);
                if (shadowratio != 1)
                {
                    curcolor = new FastColor(color.A,
                        (int)(color.R * shadowratio),
                        (int)(color.G * shadowratio),
                        (int)(color.B * shadowratio));
                }
                int sidetexture = data.GetTileTextureId(tiletype, TileSide.Back);
                int tilecount = GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratio, TileSide.Back);
                VerticesIndices toreturn = GetToReturn(tt, sidetexture);
                RectangleF texrec = TextureAtlas.TextureCoords1d(sidetexture, terrainrenderer.terrainTexturesPerAtlas, tilecount);
                short lastelement = (short)toreturn.vertices.Count;
                toreturn.vertices.Add(new VertexPositionTexture(x + 1 - flowerfix, z + 0, y + 0, texrec.Right, texrec.Bottom, curcolor));
                toreturn.vertices.Add(new VertexPositionTexture(x + 1 - flowerfix, z + 0, y + 1 * tilecount, texrec.Left, texrec.Bottom, curcolor));
                toreturn.vertices.Add(new VertexPositionTexture(x + 1 - flowerfix, z + blockheight10, y + 0, texrec.Right, texrec.Top, curcolor));
                toreturn.vertices.Add(new VertexPositionTexture(x + 1 - flowerfix, z + blockheight11, y + 1 * tilecount, texrec.Left, texrec.Top, curcolor));
                toreturn.indices.Add((ushort)(lastelement + 1));
                toreturn.indices.Add((ushort)(lastelement + 0));
                toreturn.indices.Add((ushort)(lastelement + 2));
                toreturn.indices.Add((ushort)(lastelement + 3));
                toreturn.indices.Add((ushort)(lastelement + 1));
                toreturn.indices.Add((ushort)(lastelement + 2));
            }
            if (drawleft)
            {
                curcolor = colorShadowSide;
                float shadowratio = GetShadowRatio(xx, yy - 1, zz, x, y - 1, z);
                if (shadowratio != 1)
                {
                    curcolor = new FastColor(color.A,
                        (int)(Math.Min(curcolor.R, color.R * shadowratio)),
                        (int)(Math.Min(curcolor.G, color.G * shadowratio)),
                        (int)(Math.Min(curcolor.B, color.B * shadowratio)));
                }

                int sidetexture = data.GetTileTextureId(tiletype, TileSide.Left);
                int tilecount = GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratio, TileSide.Left);
                VerticesIndices toreturn = GetToReturn(tt, sidetexture);
                RectangleF texrec = TextureAtlas.TextureCoords1d(sidetexture, terrainrenderer.terrainTexturesPerAtlas, tilecount);
                short lastelement = (short)toreturn.vertices.Count;
                toreturn.vertices.Add(new VertexPositionTexture(x + 0, z + 0, y + 0 + flowerfix, texrec.Right, texrec.Bottom, curcolor));
                toreturn.vertices.Add(new VertexPositionTexture(x + 0, z + blockheight00, y + 0 + flowerfix, texrec.Right, texrec.Top, curcolor));
                toreturn.vertices.Add(new VertexPositionTexture(x + 1 * tilecount, z + 0, y + 0 + flowerfix, texrec.Left, texrec.Bottom, curcolor));
                toreturn.vertices.Add(new VertexPositionTexture(x + 1 * tilecount, z + blockheight10, y + 0 + flowerfix, texrec.Left, texrec.Top, curcolor));
                toreturn.indices.Add((ushort)(lastelement + 0));
                toreturn.indices.Add((ushort)(lastelement + 1));
                toreturn.indices.Add((ushort)(lastelement + 2));
                toreturn.indices.Add((ushort)(lastelement + 1));
                toreturn.indices.Add((ushort)(lastelement + 3));
                toreturn.indices.Add((ushort)(lastelement + 2));
            }
            //right - same as left, but y is 1 greater.
            if (drawright)
            {
                curcolor = colorShadowSide;
                float shadowratio = GetShadowRatio(xx, yy + 1, zz, x, y + 1, z);
                if (shadowratio != 1)
                {
                    curcolor = new FastColor(color.A,
                        (int)(Math.Min(curcolor.R, color.R * shadowratio)),
                        (int)(Math.Min(curcolor.G, color.G * shadowratio)),
                        (int)(Math.Min(curcolor.B, color.B * shadowratio)));
                }

                int sidetexture = data.GetTileTextureId(tiletype, TileSide.Right);
                int tilecount = GetTilingCount(currentChunk, xx, yy, zz, tt, x, y, z, shadowratio, TileSide.Right);
                VerticesIndices toreturn = GetToReturn(tt, sidetexture);
                RectangleF texrec = TextureAtlas.TextureCoords1d(sidetexture, terrainrenderer.terrainTexturesPerAtlas, tilecount);
                short lastelement = (short)toreturn.vertices.Count;
                toreturn.vertices.Add(new VertexPositionTexture(x + 0, z + 0, y + 1 - flowerfix, texrec.Left, texrec.Bottom, curcolor));
                toreturn.vertices.Add(new VertexPositionTexture(x + 0, z + blockheight01, y + 1 - flowerfix, texrec.Left, texrec.Top, curcolor));
                toreturn.vertices.Add(new VertexPositionTexture(x + 1 * tilecount, z + 0, y + 1 - flowerfix, texrec.Right, texrec.Bottom, curcolor));
                toreturn.vertices.Add(new VertexPositionTexture(x + 1 * tilecount, z + blockheight11, y + 1 - flowerfix, texrec.Right, texrec.Top, curcolor));
                toreturn.indices.Add((ushort)(lastelement + 1));
                toreturn.indices.Add((ushort)(lastelement + 0));
                toreturn.indices.Add((ushort)(lastelement + 2));
                toreturn.indices.Add((ushort)(lastelement + 3));
                toreturn.indices.Add((ushort)(lastelement + 1));
                toreturn.indices.Add((ushort)(lastelement + 2));
            }
        }
        private int GetTilingCount(byte[, ,] currentChunk, int xx, int yy, int zz, byte tt, int x, int y, int z, float shadowratio, TileSide dir)
        {
            if (dir == TileSide.Top || dir == TileSide.Bottom)
            {
                int shadowz = dir == TileSide.Top ? 1 : -1;
                int newxx = xx + 1;
                for (; ; )
                {
                    if (newxx >= chunksize + 1) { break; }
                    if (currentChunk[newxx, yy, zz] != tt) { break; }
                    float shadowratio2 = GetShadowRatio(newxx, yy, zz + shadowz, x + (newxx - xx), y, z + shadowz);
                    if (shadowratio != shadowratio2) { break; }
                    currentChunkIgnore[newxx - 1, yy - 1, zz - 1, (int)dir] = true;
                    newxx++;
                }
                return newxx - xx;
            }
            else if (dir == TileSide.Front || dir == TileSide.Back)
            {
                int shadowx = dir == TileSide.Front ? -1 : 1;
                int newyy = yy + 1;
                for (; ; )
                {
                    if (newyy >= chunksize + 1) { break; }
                    if (currentChunk[xx, newyy, zz] != tt) { break; }
                    float shadowratio2 = GetShadowRatio(xx + shadowx, newyy, zz, x + shadowx, y + (newyy - yy), z);
                    if (shadowratio != shadowratio2) { break; }
                    currentChunkIgnore[xx - 1, newyy - 1, zz - 1, (int)dir] = true;
                    newyy++;
                }
                return newyy - yy;
            }
            else
            {
                int shadowy = dir == TileSide.Left ? -1 : 1;
                int newxx = xx + 1;
                for (; ; )
                {
                    if (newxx >= chunksize + 1) { break; }
                    if (currentChunk[newxx, yy, zz] != tt) { break; }
                    float shadowratio2 = GetShadowRatio(newxx, yy + shadowy, zz, x + (newxx - xx), y + shadowy, z);
                    if (shadowratio != shadowratio2) { break; }
                    currentChunkIgnore[newxx - 1, yy - 1, zz - 1, (int)dir] = true;
                    newxx++;
                }
                return newxx - xx;
            }
        }
        private VerticesIndices GetToReturn(byte tiletype, int textureid)
        {
            if (ENABLE_ATLAS1D)
            {
                if (!(data.IsTransparentTile(tiletype) || data.IsWaterTile(tiletype)))
                {
                    return toreturnatlas1d[textureid / terrainrenderer.terrainTexturesPerAtlas];
                }
                else
                {
                    return toreturnatlas1dtransparent[textureid / terrainrenderer.terrainTexturesPerAtlas];
                }
            }
            else
            {
                if (!(data.IsTransparentTile(tiletype) || data.IsWaterTile(tiletype)))
                {
                    return toreturnmain;
                }
                else
                {
                    return toreturntransparent;
                }
            }
        }
        bool IsTileEmptyForDrawingOrTransparent(int xx, int yy, int zz, int adjacenttiletype, byte[, ,] currentChunk)
        {
            byte tt = currentChunk[xx, yy, zz];
            if (!config3d.ENABLE_TRANSPARENCY)
            {
                return tt == data.TileIdEmpty;
            }
            return tt == data.TileIdEmpty
                || (data.IsWaterTile(tt)
                 && (!data.IsWaterTile(adjacenttiletype)))
                || data.IsTransparentTile(tt);
        }
        float GetShadowRatio(int xx, int yy, int zz, int globalx, int globaly, int globalz)
        {
            if (float.IsNaN(currentChunkShadows[xx, yy, zz]))
            {
                if (IsValidPos(globalx, globaly, globalz))
                {
                    currentChunkShadows[xx, yy, zz] = (float)mapstorage.GetLight(globalx, globaly, globalz)
                        / mapstorage.LightMaxValue();
                }
                else
                {
                    currentChunkShadows[xx, yy, zz] = 1;
                }
            }
            return currentChunkShadows[xx, yy, zz];
        }
        private bool CanSupportTorch(byte blocktype)
        {
            return blocktype != data.TileIdEmpty;
        }
    }
}