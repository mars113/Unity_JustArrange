/**************************************************
* Description
*
* 
**************************************************/

namespace MarsCode
{
    using UnityEngine;

    public class JustMatrix : MonoBehaviour
    {

        public GameObject go;

        [SerializeField]
        float width;

        [SerializeField]
        float height;

        [SerializeField]
        int columns;

        [SerializeField]
        int rows;

        [SerializeField]
        float bend;

        [SerializeField]
        float steps;

        [SerializeField]
        float ladder;

        public Vector3[,] positions;

        public Quaternion[,] rotations;

        public GameObject[] clone;

    }
}