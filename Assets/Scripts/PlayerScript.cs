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

    [CmdName("rot")]
    private class MessageRot : MessageCmdData {
        public Vector3 rot = new Vector3();
    }

    void InitializeNetPlayer(SpawnInfo spawnInfo) {
        m_netPlayer = spawnInfo.netPlayer;
        m_netPlayer.OnDisconnect += Remove;

        // Setup events for the different messages.
        m_netPlayer.RegisterCmdHandler<MessageSetName>(OnSetName);
        m_netPlayer.RegisterCmdHandler<MessageBusy>(OnBusy);
        m_netPlayer.RegisterCmdHandler<MessageRot>(OnRot);

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

    private void OnRot(MessageRot data) {
        transform.localEulerAngles = data.rot;
    }

    public Transform midTransform;
    public Transform meshTransform;

    private NetPlayer m_netPlayer;
    private Vector3 m_position;
    private string m_name;
}

}  // namespace HappyFunTimesExample

