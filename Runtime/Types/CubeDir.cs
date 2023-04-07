using UnityEngine;

namespace Kutil {

#if UNITY_EDITOR
    [UnityEditor.CustomPropertyDrawer(typeof(CubeDir))]
    public class CubeDirDrawer : ShowAsChildPropertyDrawer {
        public override string childName => nameof(CubeDir.dir);
    }
#endif
    /// <summary>
    /// 6 way direction
    /// </summary>

    [System.Serializable]
    public class CubeDir {
        public enum Dir {
            Forward, Back, Left, Right, Up, Down,
        }
        public Dir dir = Dir.Forward;

        public Vector3Int GetVec3Int => vecs[(int)dir];

        static Vector3Int[] vecs = new Vector3Int[]{
                Vector3Int.forward,
                Vector3Int.back,
                Vector3Int.left,
                Vector3Int.right,
                Vector3Int.up,
                Vector3Int.down,
            };

        public CubeDir(Dir dir) {
            this.dir = dir;
        }

        public CubeDir Flipped() {
            switch (dir) {
                case Dir.Forward:
                    return Dir.Back;
                case Dir.Back:
                    return Dir.Forward;
                case Dir.Left:
                    return Dir.Right;
                case Dir.Right:
                    return Dir.Left;
                case Dir.Up:
                    return Dir.Down;
                case Dir.Down:
                    return Dir.Up;
            }
            return default;
        }

        public CubeDir RotatedCW(int turns = 1, Dir normal = Dir.Up) {
            CubeDir cubeDir = new CubeDir(dir);
            cubeDir.RotateCW(turns, normal);
            return cubeDir;
        }
        public void RotateCW(int turns = 1, Dir normal = Dir.Up) {
            if (turns == 0) return;
            int amount = Mathf.Abs(turns);
            amount = amount % 4;
            if (amount == 0) return;
            if (dir == normal || normal == (-this).dir) {
                Debug.LogError($"Cannot rotate {dir} around {normal}!");
                return;
            }

            bool clockwise = turns > 0;

            dir = turnInfos[(int)dir].TurnCW(normal, clockwise);
            if (amount > 1) {
                RotateCW((int)Mathf.Sign(turns) * (amount - 1), normal);
            }
        }

        public BoundsInt RotateBounds(BoundsInt bounds, Vector3Int center) {
            var dirF = GetVec3Int;
            return new BoundsInt(
                bounds.position.TranslateAround(GetVec3Int, center),
                bounds.size.TranslateDir(GetVec3Int)
            );
        }

        public Quaternion GetQuaternion() {
            return Quaternion.LookRotation(GetVec3Int);
        }

        public static CubeDir FromVecIntDir(Vector3Int dir) {
            for (int i = 0; i < vecs.Length; i++) {
                if (dir == vecs[i]) return (Dir)i;
            }
            return null;
        }

        public static CubeDir GetNearestCubeDir(Vector3 dir) {
            dir.Normalize();
            int nearest = -1;
            float nearestDist = -1;
            for (int i = 0; i < vecs.Length; i++) {
                Vector3 vec = vecs[i];
                float dist = Vector3.Dot(vec, dir);
                if (dist > nearestDist) {
                    nearestDist = dist;
                    nearest = i;
                }
            }
            if (nearest == -1) return null;
            return (Dir)nearest;
        }



        [System.Serializable]
        struct TurnInfo {
            public Dir normal;
            public Dir left;
            public Dir right;
            public Dir leftAlt;
            public Dir rightAlt;

            public TurnInfo(Dir normal, Dir right, Dir rightAlt) {
                this.normal = normal;
                this.left = -((CubeDir)right);
                this.right = right;
                this.leftAlt = -((CubeDir)rightAlt); ;
                this.rightAlt = rightAlt;
            }
            public Dir TurnCW(Dir normal, bool clockwise = true) {
                if (this.normal == normal) {
                    return clockwise ? right : left;
                } else if (this.normal == ((CubeDir)normal).Flipped()) {
                    return clockwise ? left : right;
                } else if (this.normal == right) {
                    return clockwise ? rightAlt : leftAlt;
                } else {
                    return clockwise ? leftAlt : rightAlt;
                }
            }
            public Dir TurnCCW(Dir normal) {
                return TurnCW(normal, false);
            }
        }
        // order: Forward, Back, Left, Right, Up, Down,
        static TurnInfo[] turnInfos = new TurnInfo[]{
            new(Dir.Up, Dir.Right, Dir.Down),
            new(Dir.Up, Dir.Left, Dir.Up),
            new(Dir.Up, Dir.Forward, Dir.Down),
            new(Dir.Up, Dir.Back, Dir.Up),
            new(Dir.Right, Dir.Forward, Dir.Left),
            new(Dir.Right, Dir.Back, Dir.Right),
        };

        public override string ToString() {
            return dir.ToString();
        }
        public override bool Equals(object obj) {
            if (obj is CubeDir cdir && cdir != null) return dir.Equals(cdir.dir);
            return dir.Equals(obj);
        }
        public override int GetHashCode() {
            return dir.GetHashCode();
        }
        public static implicit operator CubeDir(int dir) => dir >= 0 && dir < 6 ? new CubeDir((Dir)dir) : default;
        public static implicit operator CubeDir(Dir dir) => new CubeDir(dir);
        public static implicit operator Dir(CubeDir cdir) => cdir.dir;
        public static bool operator ==(CubeDir a, CubeDir b) => a.Equals(b);
        public static bool operator !=(CubeDir a, CubeDir b) => !a.Equals(b);
        public static CubeDir operator -(CubeDir cdir) => cdir.Flipped();
        // public static CubeDir operator -(Dir cdir) => ((CubeDir)cdir).Flipped();
    }
}