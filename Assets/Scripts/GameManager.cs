using System.Collections.Generic;
using UnityEngine;

namespace HappyFunTimesExample {

// A Singlton-ish class for some global manager.
// PS: I know Singltons suck but dependency injection is hard
// in Unity.
public class GameManager : MonoBehaviour {

    public static GameManager mgr {
        get {
            return s_manager;
        }
    }

    public void AddPlayer(PlayerScript ps) {
        m_players.Add(ps);
    }

    public void RemovePlayer(PlayerScript ps) {
        m_players.Remove(ps);
    }

    public int NumPlayers {
        get {
            return m_players.Count;
        }
    }

    void Awake() {
        if (s_manager != null) {
            throw new System.InvalidProgramException("there is more than one game manager object!");
        }
        s_manager = this;
    }

    void Cleanup() {
        s_manager = null;
    }

    void OnDestroy() {
        Cleanup();
    }

    void OnApplicationExit() {
        Cleanup();
    }

    static private GameManager s_manager;

    private List<PlayerScript> m_players = new List<PlayerScript>();
}

}  // namespace HappyFunTimesExample

