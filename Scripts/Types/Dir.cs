
// public class Direction {
//     public enum Dir {
//         NORTH,
//         EAST,
//         SOUTH,
//         WEST,
//     }
//     public Dir dir;

//     static Vector3[] vecDirs = new Vector3[]{
//         Vector3.forward,
//         Vector3.right,
//         Vector3.back,
//         Vector3.left,
//     };

//     public Vector3 ToVec() {
//         return vecDirs[(int)dir];
//     }
//     public Quaternion ToRotation() {
//         return Quaternion.LookRotation(ToVec(), Vector3.up);
//     }
//     public Dir Rotated(Dir amount) {
//         return Rotated((int)amount);
//     }
//     public Dir Rotated(int turns) {
//         int nd = (int)dir + turns;
//         while (nd < 0) nd += 4;
//         while (nd > 3) nd -= 4;
//         return (Dir)nd;
//     }
//     public static Vector3 RotateAround(Vector3 vec, Vector3 center, Dir turns) {
//         return default;
//     }
// }