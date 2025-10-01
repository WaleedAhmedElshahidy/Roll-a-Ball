using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System.Data;
using UnityEngine.SceneManagement;


public class GetInputData : MonoBehaviour
{
    public InputField usernameInput;
    public InputField passwordInput;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
//public void ReadStringInput(int s)
    public void login()
    {
        // Get the username and password from the input fields.
        string username = usernameInput.text;
        string password = passwordInput.text;
        Debug.Log ("You have clicked the button from login!");
        // Get the path to the CSV file.
        string path = Application.dataPath + "/Login/csv/Students_Data.csv";

        // Open the CSV file.
        TextReader reader = File.OpenText(path);
    

        // Read the CSV file line by line.
        string line;
        while ((line = reader.ReadLine()) != null) {
            // Split the line into columns.
            string[] columns = line.Split(',');

            // Check if the username and password match the values in the CSV file.
            if (columns[0] == username && columns[1] == password) {
                // The login was successful.
                Debug.Log("Login successful!");
                // You can now login the user and grant them access to the game.
                return;
            }
        }
        // The login failed.
        Debug.Log("Login failed!");
    }
    public void switchScenes()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void LoadSelectionScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(5);
    }
}


