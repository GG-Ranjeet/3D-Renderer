using LoadObj;

// unused code

namespace InputAssembler
{
    public struct VertexData
    {
        public int id;
        public List<Vec3> position;
        public List<Vec2> uv;
        public List<Vec3> normal;

        private readonly List<int> _faceIndices = [];

        public VertexData(List<Vec3> position, List<Vec2> uv, List<Vec3> normal)
        {
            this.position = position;
            this.uv = uv;
            this.normal = normal;
        }        
    }    
}