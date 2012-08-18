using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using ManicDigger.Collisions;
using System.Drawing;
using System.Threading;
using OpenTK.Graphics.OpenGL;
using System.IO;

namespace ManicDigger
{
    public class TextureAtlasRef
    {
        public int TextureId;
        public int TextureInAtlasId;
    }
    public interface ITerrainRenderer
    {
        void Start();
        void Draw();
        void UpdateAllTiles();
        void UpdateTile(int x, int y, int z);
        int TrianglesCount();
        int texturesPacked { get; }
        int terrainTexture { get; }
        int DrawDistance { get; set; }
        int ChunkUpdates { get; }
        int[] terrainTextures1d { get; }
        int terrainTexturesPerAtlas { get; }
    }
    public class TerrainDrawerDummy : ITerrainRenderer
    {
        #region ITerrainDrawer Members
        public void Start()
        {
        }
        public void Draw()
        {
        }
        public void UpdateAllTiles()
        {
        }
        public void UpdateTile(int x, int y, int z)
        {
        }
        public int TrianglesCount()
        {
            return 0;
        }
        public int texturesPacked { get; set; }
        public int terrainTexture { get; set; }
        public int DrawDistance { get; set; }
        #endregion
        #region ITerrainDrawer Members
        public int ChunkUpdates { get; set; }
        #endregion
        #region ITerrainRenderer Members
        public int[] terrainTextures1d { get; set; }
        public int terrainTexturesPerAtlas { get; set; }
        #endregion
    }
    public class TextureAtlas
    {
        public static RectangleF TextureCoords2d(int textureId, int texturesPacked)
        {
            RectangleF r = new RectangleF();
            r.Y = (1.0f / texturesPacked * (int)(textureId / texturesPacked));
            r.X = (1.0f / texturesPacked * (textureId % texturesPacked));
            r.Width = 1.0f / texturesPacked;
            r.Height = 1.0f / texturesPacked;
            return r;
        }
        public static RectangleF TextureCoords1d(int textureId, int texturesPerAtlas)
        {
            return TextureCoords1d(textureId, texturesPerAtlas, 1);
        }
        public static RectangleF TextureCoords1d(int textureId, int texturesPerAtlas, int tilecount)
        {
            RectangleF r = new RectangleF();
            r.Y = (1.0f / texturesPerAtlas * (int)(textureId % texturesPerAtlas));
            r.X = 0;
            r.Width = 1.0f * tilecount;
            r.Height = 1.0f / texturesPerAtlas;
            return r;
        }
    }
    public enum TodoAction
    {
        Add,
        Delete,
        Clear,
    }
    public struct TodoItem
    {
        public TodoAction action;
        public Point position;
    }
    public class Vbo
    {
        public int VboID, EboID, NumElements;
        public Box3D box;
        public int realindicescount = 0;
        public int realverticescount = 0;
        public bool valid;
    }
    public class MyLinq
    {
        public static bool Any<T>(IEnumerable<T> l)
        {
            return l.GetEnumerator().MoveNext();
        }
        //public static T First<T>(IEnumerable<T> l)
        //{
        //    var e = l.GetEnumerator();
        //    e.MoveNext();
        //    return e.Current;
        //}
        //public static int Count<T>(IEnumerable<T> l)
        //{
        //    int count = 0;
        //    foreach (T v in l)
        //    {
        //        count++;
        //    }
        //    return count;
        //}
    }
    public interface ITerrainInfo
    {
        int GetTerrainBlock(int x, int y, int z);
        FastColor GetTerrainBlockColor(int x, int y, int z);
        int MapSizeX { get; }
        int MapSizeY { get; }
        int MapSizeZ { get; }
        int GetLight(int globalx, int globaly, int globalz);
        float LightMaxValue();
    }
    public interface IIsChunkReady
    {
        bool IsChunkReady(int x, int y, int z);
    }
    public class IsChunkReadyDummy : IIsChunkReady
    {
        #region IIsChunkReady Members
        public bool IsChunkReady(int x, int y, int z)
        {
            return true;
        }
        #endregion
    }
    /// <summary>
    /// </summary>
    /// <remarks>
    /// Requires OpenTK.
    /// </remarks>
    public class TerrainRenderer : ITerrainRenderer, IDisposable
    {
        [Inject]
        public IThe3d the3d { get; set; }
        [Inject]
        public IFrustumCulling frustumculling { get; set; }
        [Inject]
        public IGetFilePath getfile { get; set; }
        [Inject]
        public Config3d config3d { get; set; }
        [Inject]
        public ITerrainInfo mapstorage { get; set; }
        [Inject]
        public IIsChunkReady ischunkready { get; set; }
        [Inject]
        public IGameData data { get; set; }
        [Inject]
        public IGameExit exit { get; set; }
        [Inject]
        public ILocalPlayerPosition localplayerposition { get; set; }
        [Inject]
        public IWorldFeaturesDrawer worldfeatures { get; set; }
        [Inject]
        public TerrainChunkRenderer terrainchunkdrawer { get; set; }
        [Inject]
        public TextureAtlasConverter textureatlasconverter = new TextureAtlasConverter();
        public event EventHandler<ExceptionEventArgs> OnCrash;

        public int chunksize = 16;
        public bool DONOTDRAWEDGES = true;
        public int texturesPacked { get { return 16; } } //16x16
        public int terrainTexture { get; set; }

        public class RenderedChunk
        {
            public int[] ids;
            public bool dirty = true;
        }
        RenderedChunk[] RendererMap;
        public int chunkdrawdistance
        {
            get
            {
                int dd = drawdistance;
                dd = dd - dd % chunksize;
                //dd = ((dd * 2) / chunksize) - 1;
                dd = dd / chunksize;
                if (dd < 1)
                {
                    dd = 1;
                }
                return dd;
            }
        }
        int drawdistance = 256;
        public int DrawDistance { get { return drawdistance; } set { drawdistance = value; } }
        bool started = false;
        #region ITerrainDrawer Members
        public void Start()
        {
            if (started)
            {
                throw new Exception("Started already.");
            }
            started = true;
            GL.Enable(EnableCap.Texture2D);
            terrainTexture = the3d.LoadTexture(getfile.GetFile("terrain.png"));
            List<int> terrainTextures1d = new List<int>();
            using (var atlas2d = new Bitmap(getfile.GetFile("terrain.png")))
            {
                terrainTexturesPerAtlas = atlas1dheight / (atlas2d.Width / atlas2dtiles);
                List<Bitmap> atlases1d = textureatlasconverter.Atlas2dInto1d(atlas2d, atlas2dtiles, atlas1dheight);
                foreach (Bitmap bmp in atlases1d)
                {
                    terrainTextures1d.Add(the3d.LoadTexture(bmp));
                    bmp.Dispose();
                }
            }
            this.terrainTextures1d = terrainTextures1d.ToArray();
            updateThreadRunning++;
            new Thread(UpdateThreadStart).Start();
        }
        public int atlas1dheight = 2048;
        public int atlas2dtiles = 16; // 16x16
        public int[] terrainTextures1d { get; set; }
        public int terrainTexturesPerAtlas { get; set; }
        bool exit2;
        void UpdateThreadStart()
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                UpdateThread();
            }
            else
            {
                try
                {
                    UpdateThread();
                }
                catch (Exception e)
                {
                    if (OnCrash != null)
                    {
                        OnCrash(this, new ExceptionEventArgs() { exception = e });
                    }
                    throw;
                }
            }
        }
        public class ExceptionEventArgs : EventArgs
        {
            public Exception exception;
        }
        int updateThreadRunning = 0;
        void UpdateThread()
        {
            if (updateThreadRunning > 1)
            {
                throw new Exception("Update thread is running already.");
            }
            DoTasks();
        }

        void DoTasks()
        {
            bool done = true;
            for (; ; )
            {
                if (done)
                {
                    done = false;
                    if (exit.exit || exit2) { break; }
                    Point playerpoint = new Point((int)(localplayerposition.LocalPlayerPosition.X / chunksize), (int)(localplayerposition.LocalPlayerPosition.Z / chunksize));
                    ProcessAllPriorityTodos();
                    List<TodoItem> l = new List<TodoItem>();
                    lock (terrainlock)
                    {
                        FindChunksToDelete(l);
                        FindChunksToAdd(l);
                    }
                    //without local cache, sort could crash when player moves during sort.
                    localplayerpositioncache = localplayerposition.LocalPlayerPosition;
                    l.Sort(FTodo);
                    int max = 5;
                    for (int i = 0; i < Math.Min(max, l.Count); i++)//l.Count; i++)
                    {
                        var ti = l[i];
                        if (exit.exit || exit2) { break; }
                        ProcessAllPriorityTodos();
                        if (!ProcessUpdaterTodo(ti)) { max++; }
                    }
                    l.Clear();
                    done = true;
                }
            }
        }
        
        private void FindChunksToDelete(List<TodoItem> l)
        {
            foreach (var k in batchedblocks.Values)
            {
                var v = k.V;
                if ((new Vector3(v.X * chunksize, localplayerposition.LocalPlayerPosition.Y, v.Y * chunksize) - localplayerposition.LocalPlayerPosition).Length > chunkdrawdistance * chunksize)
                {
                    l.Add(new TodoItem() { position = new Point((int)v.X, (int)v.Y), action = TodoAction.Delete });
                }
            }
        }
        private void FindChunksToAdd(List<TodoItem> l)
        {
            for (int x = -chunkdrawdistance; x <= chunkdrawdistance; x++)
            {
                for (int y = -chunkdrawdistance; y <= chunkdrawdistance; y++)
                {
                    int xx = (int)localplayerposition.LocalPlayerPosition.X / chunksize + x;
                    int yy = (int)localplayerposition.LocalPlayerPosition.Z / chunksize + y;
                    bool add = false;
                    for (int z = 0; z < mapstorage.MapSizeZ / chunksize; z++)
                    {
                        if (!batchedblocks.ContainsKey(GetV3HashCode(xx, yy, z))) //if block isnt in the batcher
                        {
                            add = true; //allow the add
                            break;
                        }
                    }
                    //check if can be added and add
                    if (add && (new Vector3(xx * chunksize, localplayerposition.LocalPlayerPosition.Y, yy * chunksize) - localplayerposition.LocalPlayerPosition).Length <= chunkdrawdistance * chunksize
                        && IsValidChunkPosition(xx, yy))
                    {
                        l.Add(new TodoItem() { position = new Point(xx, yy), action = TodoAction.Add });
                    }
                }
            }
        }
        private bool IsValidChunkPosition(int xx, int yy)
        {
            return xx >= 0 && yy >= 0 && xx < mapstorage.MapSizeX / chunksize && yy < mapstorage.MapSizeY / chunksize;
        }
        private bool IsValidChunkPosition(int xx, int yy, int zz)
        {
            return xx >= 0 && yy >= 0 && zz >= 0
                && xx < mapstorage.MapSizeX / chunksize
                && yy < mapstorage.MapSizeY / chunksize
                && zz < mapstorage.MapSizeZ / chunksize;
        }
        Vector3 localplayerpositioncache;
        int FTodo(TodoItem a, TodoItem b)
        {
            if (a.action == TodoAction.Delete && b.action == TodoAction.Add)
            {
                return -1;
            }
            if (a.action == TodoAction.Add && b.action == TodoAction.Delete)
            {
                return 1;
            }
            return FPoint(a.position, b.position);
        }
        int FPoint(Point a, Point b)
        {
            return ((new Vector3(a.X * chunksize, localplayerpositioncache.Y, a.Y * chunksize) - localplayerpositioncache).Length)
                .CompareTo((new Vector3(b.X * chunksize, localplayerpositioncache.Y, b.Y * chunksize) - localplayerpositioncache).Length);
        }
        int FVector3Arr(Vector3[] a, Vector3[] b)
        {
            return ((Vector3.Multiply(a[0], chunksize) - localplayerposition.LocalPlayerPosition).Length)
                .CompareTo((Vector3.Multiply(b[0], chunksize) - localplayerposition.LocalPlayerPosition).Length);
        }
        int FVector3(Vector3 a, Vector3 b)
        {
            return ((Vector3.Multiply(a, chunksize) - localplayerposition.LocalPlayerPosition).Length)
                .CompareTo((Vector3.Multiply(b, chunksize) - localplayerposition.LocalPlayerPosition).Length);
        }
        int FVector3Chunk(Vector3 a, Vector3 b)
        {
            return ((new Vector3(a.X * chunksize, a.Z * chunksize, a.Y * chunksize) - localplayerposition.LocalPlayerPosition).Length)
                .CompareTo((new Vector3(b.X * chunksize, b.Z * chunksize, b.Y * chunksize) - localplayerposition.LocalPlayerPosition).Length);
        }
        int MAX_PRIORITY_UPDATES = 5;
        private void ProcessAllPriorityTodos()
        {
            int done = 0;
            while (prioritytodo.Count > 0)
            {
                done++;
                if (done > MAX_PRIORITY_UPDATES)
                {
                    break;
                }
                if (exit.exit || exit2) { return; }
                Vector3[] ti;
                lock (prioritytodo)
                {
                    prioritytodo.Sort(FVector3Arr);
                    //todo remove duplicates
                    ti = prioritytodo[0];//.Dequeue();
                    prioritytodo.RemoveAt(0);
                }
                //Prepare list of near chunks to update.
                //This is the slowest part.
                Dictionary<Vector3, VerticesIndicesToLoad[]> nearchunksadd = new Dictionary<Vector3, VerticesIndicesToLoad[]>();
                List<Vector3> nearchunksremove = new List<Vector3>();
                for (int i = 0; i < ti.Length; i++)
                {
                    var p = ti[i];
                    if (InFrustum((int)p.X, (int)p.Y, (int)p.Z))
                    {
                        var chunk = MakeChunk((int)p.X, (int)p.Y, (int)p.Z);
                        var chunkk = new List<VerticesIndicesToLoad>(chunk);
                        if (chunkk.Count() > 0)
                        {
                            nearchunksadd.Add(p, chunkk.ToArray());
                        }
                        if (batchedblocks.ContainsKey(GetV3HashCode((int)p.X, (int)p.Y, (int)p.Z)))
                        {
                            nearchunksremove.Add(p);
                        }
                    }
                }
                //Update all near chunks at the same time, for flicker-free drawing.
                lock (terrainlock)
                {
                    foreach (Vector3 p in nearchunksremove)
                    {
                        if (!batchedblocks.ContainsKey(GetV3HashCode((int)p.X, (int)p.Y, (int)p.Z)))
                        {
                            //crash fix. crash happens on UpdateAllTiles().
                            continue;
                        }
                        int[] b = batchedblocks[GetV3HashCode((int)p.X, (int)p.Y, (int)p.Z)].A;
                        if (b != null)
                        {
                            foreach (int id in b)
                            {
                                batcher.Remove(id);
                            }
                        }
                        batchedblocks.Remove(GetV3HashCode((int)p.X, (int)p.Y, (int)p.Z));
                    }
                    foreach (var k in nearchunksadd)
                    {
                        var p = k.Key;
                        var chunk = k.Value;
                        List<int> ids = new List<int>();
                        foreach (var cc in chunk)
                        {
                            Vector3 center = new Vector3(cc.position.X + chunksize / 2, cc.position.Z + chunksize / 2, cc.position.Y + chunksize / 2);
                            float radius = chunksize;
                            ids.Add(batcher.Add(cc.indices, cc.vertices, cc.transparent, cc.texture, center, radius));
                        }
                        batchedblocks[GetV3HashCode((int)p.X, (int)p.Y, (int)p.Z)] = new BlockData() { V = p, A = ids.ToArray() };
                    }
                }
            }
        }
        private bool ProcessUpdaterTodo(TodoItem ti)
        {
            var p = ti.position;
            bool processed = false;
            if (ti.action == TodoAction.Add)
            {
                for (int z = 0; z < mapstorage.MapSizeZ / chunksize; z++)
                {
                    try
                    {
                        //lock (terrainlock)
                        {
                            if (batchedblocks.ContainsKey(GetV3HashCode(p.X, p.Y, z)))
                            {
                                continue;
                            }
                            if (!IsChunksAroundReady(p.X, p.Y, z))
                            {
                                continue;
                            }
                            processed = true;
                            var chunk = MakeChunk(p.X, p.Y, z);
                            //var chunkk = new List<VerticesIndicesToLoad>(chunk);
                            List<int> ids = new List<int>();
                            foreach (VerticesIndicesToLoad v in chunk)
                            {
                                if (v.indices.Length != 0)
                                {
                                    Vector3 center = new Vector3(v.position.X + chunksize / 2, v.position.Z + chunksize / 2, v.position.Y + chunksize / 2);
                                    float radius = chunksize;
                                    ids.Add(batcher.Add(v.indices, v.vertices, v.transparent, v.texture, center, radius));
                                }
                            }
                            if (ids.Count > 0)
                            {
                                batchedblocks[GetV3HashCode(p.X, p.Y, z)] = new BlockData() { V = new Vector3(p.X, p.Y, z), A = ids.ToArray() };
                            }
                            else
                            {
                                batchedblocks[GetV3HashCode(p.X, p.Y, z)] = new BlockData();
                            }
                        }
                    }
                    catch { Console.WriteLine("Chunk error"); }
                }
            }
            else if (ti.action == TodoAction.Delete)
            {
                processed = true;
                for (int z = 0; z < mapstorage.MapSizeZ / chunksize; z++)
                {
                    //lock (terrainlock)
                    {
                        BlockData bd;
                        if (batchedblocks.TryGetValue(GetV3HashCode(p.X, p.Y, z), out bd))
                        {
                            if (bd.A != null)
                            {
                                foreach (int id in bd.A)
                                {
                                    batcher.Remove(id);
                                }
                            }
                            batchedblocks.Remove(GetV3HashCode(p.X, p.Y, z));
                        }
                    }
                }
            }
            else
            {
                processed = true;
                UpdateAllTiles();
            }
            return processed;
        }
        private bool IsChunksAroundReady(int chunkx, int chunky, int achunkz)
        {
            //Bad fix
            //Was: Just making sure that 6 chunks around are loaded - for drawing edges of chunk.
            //Is: Forced whole columns to be loaded, so that when using full shadows,
            //light does not flood into not yet ready chunks which would cause 
            //UpdateStartSunlight() to mark a whole column of chunks as fully-lighted.
            for (int zz = 0; zz < mapstorage.MapSizeZ / chunksize; zz++)
            //int zz = achunkz;
            {
                int chunkz = zz;
                int x = chunkx * chunksize;
                int y = chunky * chunksize;
                int z = chunkz * chunksize;
                if (!(ischunkready.IsChunkReady(x, y, z)))
                {
                    return false;
                }

                if (IsValidChunkPosition(chunkx - 1, chunky, chunkz) && !ischunkready.IsChunkReady(x - chunksize, y, z))
                {
                    return false;
                }
                if (IsValidChunkPosition(chunkx + 1, chunky, chunkz) && !ischunkready.IsChunkReady(x + chunksize, y, z))
                {
                    return false;
                }
                if (IsValidChunkPosition(chunkx, chunky - 1, chunkz) && !ischunkready.IsChunkReady(x, y - chunksize, z))
                {
                    return false;
                }
                if (IsValidChunkPosition(chunkx, chunky + 1, chunkz) && !ischunkready.IsChunkReady(x, y + chunksize, z))
                {
                    return false;
                }

                if (IsValidChunkPosition(chunkx - 1, chunky - 1, chunkz) && !ischunkready.IsChunkReady(x - chunksize, y - chunksize, z))
                {
                    return false;
                }
                if (IsValidChunkPosition(chunkx - 1, chunky + 1, chunkz) && !ischunkready.IsChunkReady(x - chunksize, y + chunksize, z))
                {
                    return false;
                }
                if (IsValidChunkPosition(chunkx + 1, chunky - 1, chunkz) && !ischunkready.IsChunkReady(x + chunksize, y - chunksize, z))
                {
                    return false;
                }
                if (IsValidChunkPosition(chunkx + 1, chunky + 1, chunkz) && !ischunkready.IsChunkReady(x + chunksize, y + chunksize, z))
                {
                    return false;
                }
            }
            return true;
        }
        private IEnumerable<VerticesIndicesToLoad> MakeChunk(int x, int y, int z)
        {
            chunkupdates++;
            return terrainchunkdrawer.MakeChunk(x, y, z);
        }
        int chunkupdates = 0;
        public int ChunkUpdates { get { return chunkupdates; } }
        /// <summary>
        /// Contains chunk positions.
        /// </summary>
        //Queue<Vector3> prioritytodo = new Queue<Vector3>();
        List<Vector3[]> prioritytodo = new List<Vector3[]>();
        object terrainlock = new object();
        public void Draw()
        {
            //GL.Color3(terraincolor);
            worldfeatures.DrawWorldFeatures();
            lock (terrainlock)
            {
                GL.BindTexture(TextureTarget.Texture2D, terrainTexture);
                //must be drawn last, for transparent blocks.
                batcher.Draw(localplayerposition.LocalPlayerPosition);
            }
        }
        private struct BlockData
        {
            public Vector3 V;
            public int[] A;
        }
        Dictionary<int, BlockData> batchedblocks = new Dictionary<int, BlockData>();
        public static int GetV3HashCode(int i, int j, int k)
        {
            return (int)(0xE75C6973 * (uint)i + 0xC965737D * (uint)j + 0x7EC976D5 * (uint)k);
        }
        private bool InFrustum(int x, int y, int z)
        {
           return frustumculling.SphereInFrustum(x * chunksize + chunksize / 2, z * chunksize + chunksize / 2, y * chunksize + chunksize / 2, chunksize);
        }

        Vector3 lastplayerposition;
        public MeshBatcher batcher;
        public void UpdateAllTiles()
        {
            lock (terrainlock)
            {
                batchedblocks.Clear();
                batcher.Clear();
            }
            return;
        }
        enum UpdateType
        {
            Add,
            Delete,
        }
        struct ToUpdate
        {
            public UpdateType type;
            public Vector3 position;
        }
        IEnumerable<Vector3> TilesAround(Vector3 pos)
        {
            yield return new Vector3(pos + new Vector3(0, 0, 0));
            yield return new Vector3(pos + new Vector3(+1, 0, 0));
            yield return new Vector3(pos + new Vector3(-1, 0, 0));
            yield return new Vector3(pos + new Vector3(0, +1, 0));
            yield return new Vector3(pos + new Vector3(0, -1, 0));
            yield return new Vector3(pos + new Vector3(0, 0, +1));
            yield return new Vector3(pos + new Vector3(0, 0, -1));
        }
        bool IsChunkLoaded(int x, int y)
        {
            for (int z = 0; z < mapstorage.MapSizeZ / chunksize; z++)
            {
                if (batchedblocks.ContainsKey(GetV3HashCode(x, y, z)))
                {
                    return true;
                }
            }
            return false;
        }
        public void UpdateTile(int xx, int yy, int zz)
        {
            List<Vector3> l = new List<Vector3>();
            lock (prioritytodo)
            {
                if (!IsChunkLoaded(xx / chunksize, yy / chunksize))
                {
                    return;
                }
                l.Add(new Vector3(xx / chunksize, yy / chunksize, zz / chunksize));
                foreach (Vector3 t in TilesAround(new Vector3(xx, yy, zz)))
                {
                    int x = (int)t.X;
                    int y = (int)t.Y;
                    int z = (int)t.Z;
                    if (x / chunksize != xx / chunksize
                        || y / chunksize != yy / chunksize
                        || z / chunksize != zz / chunksize)
                    {
                        l.Add(new Vector3(x / chunksize, y / chunksize, z / chunksize));
                    }
                }
                prioritytodo.Add(l.ToArray());
            }
        }
        public int TrianglesCount()
        {
            return batcher.TotalTriangleCount;
        }
        #endregion
        #region IDisposable Members
        public void Dispose()
        {
            exit2 = true;
            while (updateThreadRunning > 0)
            {
                Thread.Sleep(0);
            }
        }
        #endregion
    }
    public enum TorchType
    {
        Normal, Left, Right, Front, Back
    }
    public interface IBlockDrawerTorch
    {
        void AddTorch(List<ushort> myelements, List<VertexPositionTexture> myvertices, int x, int y, int z, TorchType type);
    }
    public class BlockDrawerTorchDummy : IBlockDrawerTorch
    {
        #region IBlockDrawerTorch Members
        public void AddTorch(List<ushort> myelements, List<VertexPositionTexture> myvertices, int x, int y, int z, TorchType type)
        {
        }
        #endregion
    }
    
    public interface IWorldFeaturesDrawer
    {
        void DrawWorldFeatures();
    }
    public class WorldFeaturesDrawerDummy : IWorldFeaturesDrawer
    {
        #region IWorldFeaturesDrawer Members
        public void DrawWorldFeatures()
        {
        }
        #endregion
    }
    /// <summary>
    /// </summary>
    /// <remarks>
    /// Requires OpenTK.
    /// </remarks>
    public class WorldFeaturesDrawer : IWorldFeaturesDrawer
    {
        [Inject]
        public IThe3d the3d { get; set; }
        [Inject]
        public IGetFilePath getfile { get; set; }
        [Inject]
        public IMapStorage mapstorage { get; set; }
        [Inject]
        public ILocalPlayerPosition localplayerposition { get; set; }
        public void DrawWorldFeatures()
        {
            if (ENABLE_WATER)
            {
                DrawMapEdges();
                DrawWater();
            }
        }
        public bool ENABLE_WATER = true;
        int? watertexture;
        int? rocktexture;
        bool waternotfoundwritten = false;
        bool rocknotfoundwritten = false;
        Color terraincolor { get { return Color.White; } }//return localplayerposition.Swimming ? Color.FromArgb(255, 78, 95, 140) : Color.White; } }
        private void DrawWater()
        {
            if (waternotfoundwritten) { return; }
            if (watertexture == null)
            {
                try
                { watertexture = the3d.LoadTexture(getfile.GetFile("water.png")); }
                catch (FileNotFoundException)
                {
                    Console.WriteLine("water.png not found."); waternotfoundwritten = true; return;
                }
            }
            GL.BindTexture(TextureTarget.Texture2D, watertexture.Value);
            GL.Enable(EnableCap.Texture2D);
            //GL.Color3(terraincolor);
            GL.Color3(Color.White);
            GL.Begin(BeginMode.Quads);
            foreach (Rectangle r in AroundMap())
            {
                DrawWaterQuad(r.X, r.Y, r.Width, r.Height,
                    mapstorage.WaterLevel);
            }
            GL.End();
        }
        private void DrawMapEdges()
        {
            if (rocknotfoundwritten) { return; }
            if (rocktexture == null)
            {
                try
                { rocktexture = the3d.LoadTexture(getfile.GetFile("rock.png")); }
                catch (FileNotFoundException)
                {
                    Console.WriteLine("rock.png not found."); rocknotfoundwritten = true; return;
                }
            }
            GL.BindTexture(TextureTarget.Texture2D, rocktexture.Value);
            GL.Enable(EnableCap.Texture2D);
            //GL.Color3(terraincolor);
            GL.Color3(Color.White);
            GL.Begin(BeginMode.Quads);
            foreach (IEnumerable<Point> r in MapEdges())
            {
                List<Point> rr = new List<Point>(r);
                DrawRockQuad(rr[0].X, rr[0].Y, rr[1].X, rr[1].Y,
                    mapstorage.WaterLevel - 2);
            }
            foreach (Rectangle r in AroundMap())
            {
                DrawWaterQuad(r.X, r.Y, r.Width, r.Height,
                    mapstorage.WaterLevel - 2);
            }
            DrawWaterQuad(0, 0, mapstorage.MapSizeX, mapstorage.MapSizeY, 0);
            GL.End();
        }
        private IEnumerable<IEnumerable<Point>> MapEdges()
        {
            yield return new Point[] { new Point(0, 0), new Point(mapstorage.MapSizeX, 0) };
            yield return new Point[] { new Point(mapstorage.MapSizeX, 0), new Point(mapstorage.MapSizeX, mapstorage.MapSizeY) };
            yield return new Point[] { new Point(mapstorage.MapSizeX, mapstorage.MapSizeY), new Point(0, mapstorage.MapSizeY) };
            yield return new Point[] { new Point(0, mapstorage.MapSizeY), new Point(0, 0) };
        }
        private void DrawRockQuad(int x1, int y1, int x2, int y2, float height)
        {
            RectangleF rect = new RectangleF(0, 0, Math.Max(Math.Abs(x2 - x1), Math.Abs(y2 - y1)), height);
            GL.TexCoord2(rect.Right, rect.Bottom); GL.Vertex3(x2, 0, y2);
            GL.TexCoord2(rect.Right, rect.Top); GL.Vertex3(x2, height, y2);
            GL.TexCoord2(rect.Left, rect.Top); GL.Vertex3(x1, height, y1);
            GL.TexCoord2(rect.Left, rect.Bottom); GL.Vertex3(x1, 0, y1);
        }
        int watersizex = 1 * 1000;
        int watersizey = 1 * 1000;
        IEnumerable<Rectangle> AroundMap()
        {
            yield return new Rectangle(-watersizex, -watersizey, mapstorage.MapSizeX + watersizex * 2, watersizey);
            yield return new Rectangle(-watersizex, mapstorage.MapSizeY, mapstorage.MapSizeX + watersizex * 2, watersizey);
            yield return new Rectangle(-watersizex, 0, watersizex, mapstorage.MapSizeY);
            yield return new Rectangle(mapstorage.MapSizeX, 0, watersizex, mapstorage.MapSizeY);
        }
        void DrawWaterQuad(float x1, float y1, float width, float height, float z1)
        {
            RectangleF rect = new RectangleF(0, 0, 1 * width, 1 * height);
            float x2 = x1 + width;
            float y2 = y1 + height;
            GL.TexCoord2(rect.Right, rect.Bottom); GL.Vertex3(x2, z1, y2);
            GL.TexCoord2(rect.Right, rect.Top); GL.Vertex3(x2, z1, y1);
            GL.TexCoord2(rect.Left, rect.Top); GL.Vertex3(x1, z1, y1);
            GL.TexCoord2(rect.Left, rect.Bottom); GL.Vertex3(x1, z1, y2);
        }
    }
}