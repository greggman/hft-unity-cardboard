using UnityEngine;
using System.Collections.Generic;
using HappyFunTimes;
using CSSParse;

namespace HappyFunTimesExample {

public class PlayerScript : MonoBehaviour {
    // Classes based on MessageCmdData are automatically registered for deserialization
    // by CmdName.
    [CmdName("setName")]
    private class MessageSetName : MessageCmdData {
        public MessageSetName() {  // needed for deserialization
        }
        public MessageSetName(string _name) {
            name = _name;
        }
        public string name = "";
    };

    [CmdName("busy")]
    private class MessageBusy : MessageCmdData {
        public bool busy = false;
    }

    [CmdName("orient")]
    private class MessageOrient : MessageCmdData {
        public float alpha = 0;
        public float beta = 0;
        public float gamma = 0;
    }

    void InitializeNetPlayer(SpawnInfo spawnInfo) {
        m_netPlayer = spawnInfo.netPlayer;
        m_netPlayer.OnDisconnect += Remove;

        // Setup events for the different messages.
        m_netPlayer.RegisterCmdHandler<MessageSetName>(OnSetName);
        m_netPlayer.RegisterCmdHandler<MessageBusy>(OnBusy);
        m_netPlayer.RegisterCmdHandler<MessageOrient>(OnOrient);

        m_position = new Vector3(Random.Range(-10, 10), 0, Random.Range(-10, 10));
        if (GameManager.mgr.NumPlayers == 0) {
            m_position = new Vector3(0, 10, 0);
        }
        transform.localPosition = m_position;
        SetName(spawnInfo.name);
        GameManager.mgr.AddPlayer(this);
    }

    void Start() {
        m_position = gameObject.transform.localPosition;
    }

    public void Update() {
    }

    private void SetName(string name) {
        m_name = name;
    }

    private void Remove(object sender, System.EventArgs e) {
        GameManager.mgr.RemovePlayer(this);
        Destroy(gameObject);
    }

    private void OnSetName(MessageSetName data) {
        if (data.name.Length == 0) {
            m_netPlayer.SendCmd(new MessageSetName(m_name));
        } else {
            SetName(data.name);
        }
    }

    private void OnBusy(MessageBusy data) {
        // not used.
    }

    //Quaternion QuaternionFromMatrix(Matrix4x4 m) {
    //    // Adapted from: http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
    //    Quaternion q = new Quaternion();
    //    q.w = Mathf.Sqrt( Mathf.Max( 0, 1 + m[0,0] + m[1,1] + m[2,2] ) ) / 2;
    //    q.x = Mathf.Sqrt( Mathf.Max( 0, 1 + m[0,0] - m[1,1] - m[2,2] ) ) / 2;
    //    q.y = Mathf.Sqrt( Mathf.Max( 0, 1 - m[0,0] + m[1,1] - m[2,2] ) ) / 2;
    //    q.z = Mathf.Sqrt( Mathf.Max( 0, 1 - m[0,0] - m[1,1] + m[2,2] ) ) / 2;
    //    q.x = Mathf.Sign( q.x  ( m[2,1] - m[1,2] ) );
    //    q.y = Mathf.Sign( q.y  ( m[0,2] - m[2,0] ) );
    //    q.z = Mathf.Sign( q.z  ( m[1,0] - m[0,1] ) );
    //    return q;
    //}


    Quaternion QuatFromMatrix(Matrix4x4 m)
    {
        float trace, radicand, scale, xx, yx, zx, xy, yy, zy, xz, yz, zz, tmpx, tmpy, tmpz, tmpw, qx, qy, qz, qw;
        bool negTrace, ZgtX, ZgtY, YgtX;
        bool largestXorY, largestYorZ, largestZorX;

        xx = m[0, 0];//tfrm.getCol0().getX();
        yx = m[0, 1];//tfrm.getCol0().getY();
        zx = m[0, 2];//tfrm.getCol0().getZ();
        xy = m[1, 0];//tfrm.getCol1().getX();
        yy = m[1, 1];//tfrm.getCol1().getY();
        zy = m[1, 2];//tfrm.getCol1().getZ();
        xz = m[2, 0];//tfrm.getCol2().getX();
        yz = m[2, 1];//tfrm.getCol2().getY();
        zz = m[2, 2];//tfrm.getCol2().getZ();

        trace = ( ( xx + yy ) + zz );

        negTrace = ( trace < 0.0f );
        ZgtX = zz > xx;
        ZgtY = zz > yy;
        YgtX = yy > xx;
        largestXorY = ( !ZgtX || !ZgtY ) && negTrace;
        largestYorZ = ( YgtX || ZgtX ) && negTrace;
        largestZorX = ( ZgtY || !YgtX ) && negTrace;

        if ( largestXorY )
        {
            zz = -zz;
            xy = -xy;
        }
        if ( largestYorZ )
        {
            xx = -xx;
            yz = -yz;
        }
        if ( largestZorX )
        {
            yy = -yy;
            zx = -zx;
        }

        radicand = ( ( ( xx + yy ) + zz ) + 1.0f );
        scale = ( 0.5f * ( 1.0f / Mathf.Sqrt( radicand ) ) );

        tmpx = ( ( zy - yz ) * scale );
        tmpy = ( ( xz - zx ) * scale );
        tmpz = ( ( yx - xy ) * scale );
        tmpw = ( radicand * scale );
        qx = tmpx;
        qy = tmpy;
        qz = tmpz;
        qw = tmpw;

        if ( largestXorY )
        {
            qx = tmpw;
            qy = tmpz;
            qz = tmpy;
            qw = tmpx;
        }
        if ( largestYorZ )
        {
            tmpx = qx;
            tmpz = qz;
            qx = qy;
            qy = tmpx;
            qz = qw;
            qw = tmpz;
        }

        return new Quaternion(qx, qy, qz, qw);
    }

    private void OnOrient(MessageOrient data) {
        //m_orient = data;
        //Matrix4x4 mesh = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, -data.gamma), Vector3.one);
        //Matrix4x4 mid = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(data.beta, 0, 0), Vector3.one);
        //Matrix4x4 root = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, data.alpha, 0), Vector3.one);
        //
        ////Matrix4x4 m = root * mesh * mid;
        //Matrix4x4 m = mid * mesh * root;
        //
        //transform.localRotation = QuatFromMatrix(m);
        meshTransform.localEulerAngles = new Vector3(0, 0, -data.gamma);
        midTransform.localEulerAngles = new Vector3(data.beta, 0, 0);
        transform.localEulerAngles = new Vector3(0, -data.alpha, 0);
    }

    public Transform midTransform;
    public Transform meshTransform;

    private NetPlayer m_netPlayer;
    private Vector3 m_position;
    private string m_name;
}

}  // namespace HappyFunTimesExample

