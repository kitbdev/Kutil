using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kutil {

    /// <summary>
    /// A 2D 4 way compass direction
    /// </summary>
    [System.Serializable]
    public class Direction {
        public enum Dir {
            NORTH,
            EAST,
            SOUTH,
            WEST,
        }
        public Dir dir = Dir.NORTH;

        static Vector2Int[] vec2Dirs = new Vector2Int[]{
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left,
        };
        static Vector3Int[] vec3Dirs = new Vector3Int[]{
            Vector3Int.forward,
            Vector3Int.right,
            Vector3Int.back,
            Vector3Int.left,
        };

        public Direction() {
        }
        public Direction(Dir dir) {
            this.dir = dir;
        }

        public Vector2Int ToVec2() {
            return vec2Dirs[(int)dir];
        }
        public Vector3Int ToVec3() {
            return vec3Dirs[(int)dir];
        }
        public Quaternion ToRotationXZ() {
            return Quaternion.LookRotation(ToVec3(), Vector3.up);
        }
        public Quaternion ToRotationXY() {
            return Quaternion.LookRotation(Vector3.forward, (Vector2)ToVec2());
        }
        public static Direction FromVec3(Vector3Int vec3) {
            for (int i = 0; i < vec3Dirs.Length; i++) {
                Vector3Int v3 = vec3Dirs[i];
                if (vec3 == v3) {
                    return (Dir)i;
                }
            }
            return null;
        }
        public static Direction FromVec2(Vector2Int vec2) {
            for (int i = 0; i < vec2Dirs.Length; i++) {
                Vector2Int v2 = vec2Dirs[i];
                if (vec2 == v2) {
                    return (Dir)i;
                }
            }
            return null;
        }
        public static Direction FromRotationXZRound(Quaternion rot) {
            Direction closest = Dir.NORTH;
            float closestAng = float.MaxValue;
            foreach (var dir in vec2Dirs.Select(v2 => FromVec2(v2))) {
                float ang = Quaternion.Angle(dir.ToRotationXZ(), rot);
                if (ang < closestAng) {
                    closestAng = ang;
                    closest = dir;
                }
            }
            return closest;
        }
        public static Direction FromRotationXZ(Quaternion rot) {
            if (rot.eulerAngles.x != 0 || rot.eulerAngles.z != 0) return null;
            float ang = rot.eulerAngles.y;
            switch (ang) {
                case 0f:
                case 360f:
                    return Dir.NORTH;
                case 90f:
                    return Dir.EAST;
                case 180f:
                    return Dir.SOUTH;
                case 270f:
                    return Dir.WEST;
            }
            return null;
        }
        public static Direction FromRotationXY(Quaternion rot) {
            // todo
            throw new System.NotImplementedException();
        }
        public Direction Rotated(Direction amount) {
            return Rotated((int)amount.dir);
        }
        public Direction Rotated(int turns) {
            int nd = (int)dir + turns;
            while (nd < 0) nd += 4;
            while (nd > 3) nd -= 4;
            return (Dir)nd;
        }
        public Direction RotateLeft(int times = 1) {
            dir = this.Rotated(-times);
            return this;
        }
        public Direction RotateRight(int times = 1) {
            dir = this.Rotated(times);
            return this;
        }
        public static Vector3 RotateAround(Vector3 vec, Vector3 center, Dir turns) {
            return default;
        }
        public static implicit operator Direction(Dir dir) => new Direction(dir);
        public static implicit operator Dir(Direction dir) => dir.dir;

        public override string ToString() {
            return dir.ToString();
        }
        public override bool Equals(object obj) {
            return dir.Equals(obj);
        }
        public override int GetHashCode() {
            return dir.GetHashCode();
        }
    }
}