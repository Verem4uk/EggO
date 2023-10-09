using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayersView : MonoBehaviour
{
    [SerializeField] 
    private GameCore GameCore;
    
    [SerializeField] 
    private InputField PlayerPrefab;

    [SerializeField] 
    private List<InputField> Players;

    [SerializeField] 
    private Toggle PractisesToggle;

    [SerializeField] 
    private GameObject PlusButton;
    
    [SerializeField] 
    private GameObject MinusButton;

    private void OnEnable() => CheckMinusButton();
    private void CheckMinusButton() => MinusButton.SetActive(Players.Count != 1);
    public void AddPlayer()
    {
        Players.Add(Instantiate(PlayerPrefab, transform));
        CheckMinusButton();
    }

    public void StartGame()
    {
        var players = new string[Players.Count];
        for (int i = 0; i < players.Length; i++)
        {
            players[i] = Players[i].text;
        }
        GameCore.Initialize(players, PractisesToggle.isOn);
    }

    public void RemovePlayer()
    {
        var playerToRemove = Players[Players.Count - 1];
        Players.Remove(playerToRemove);
        Destroy(playerToRemove.gameObject);
        CheckMinusButton();
    }
}
