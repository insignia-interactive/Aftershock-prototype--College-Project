using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.SceneManagement;
using WebSocketSharp;

public class PlayfabLogin : MonoBehaviour
{
    [Header("UI")] 
    public TMP_Text messageText;
    [SerializeField] private GameObject LoginPanel;
    [SerializeField] private GameObject UsernamePanel;
    
    [Header("Login Details")]
    [SerializeField] private string username;
    [SerializeField] private string email;
    [SerializeField] private string password;
    [SerializeField] private bool remember;

    private void Start()
    {
        // Checks if connected to the correct Title on playfab | If not connected connect
        if (string.IsNullOrEmpty(PlayFabSettings.TitleId))
        {
            PlayFabSettings.TitleId = "4451A";
        }

        // Checks if user already has an email & password stored on their system
        if (PlayerPrefs.HasKey("EMAIL") && PlayerPrefs.HasKey("PASSWORD"))
        {
            // If email and password stored create a Playfab login request
            var request = new LoginWithEmailAddressRequest
            {
                Email = PlayerPrefs.GetString("EMAIL"),
                Password = PlayerPrefs.GetString("PASSWORD"),
                InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
                {
                    GetPlayerProfile = true
                }
            };
            PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnError);
        }
    }

    // Checks if username is valid
    private bool IsValidUsername()
    {
        bool IsValid = false;
        
        if (username.Length >= 3 && username.Length <= 24)
        {
            IsValid = true;
        }

        return IsValid;
    }
    
    // Register button clicked
    public void RegisterButton()
    {
        // Checks for empty email/password input
        if (email.IsNullOrEmpty() || password.IsNullOrEmpty())
        {
            messageText.text = "Email or Password empty";
            return;
        }

        // Checks if password longer than 6
        if (password.Length < 6)
        {
            messageText.text = "Password too short";
            return;
        }
        
        // Creates a register request (Creates account on playfab)
        var request = new RegisterPlayFabUserRequest
        {
            Email = email,
            Password = password,
            RequireBothUsernameAndEmail = false
        };
        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnError);
    }

    // Updates the display name on playfab
    private void UpdateDisplayName(string displayName)
    {
        var request = new UpdateUserTitleDisplayNameRequest { DisplayName = displayName };
        PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnDisplayNameSuccess, OnError);
    }

    // Login button click
    public void LoginButton()
    {
        // Sends a login request with the email & password entered
        var request = new LoginWithEmailAddressRequest
        {
            Email = email,
            Password = password,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            }
        };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnError);
    }
    
    // Reset password click
    public void ResetPasswordButton()
    {
        // Sends a reset password request to playfab (sends an account recovery email to the email entered)
        var request = new SendAccountRecoveryEmailRequest
        {
            Email = email,
            TitleId = "4451A"
        };
        PlayFabClientAPI.SendAccountRecoveryEmail(request, OnPasswordReset, OnError);
    }

    // Continue as guest button click
    public void GuestButton()
    {
        // Creates a request that creates and account using the deviceUniqueIdentifier
        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            }
        };
        PlayFabClientAPI.LoginWithCustomID(request, OnGuestLoginSuccess, OnError);
    }

    // Update username button
    public void UsernameButton()
    {
        UpdateDisplayName(username);
    }
    
    // Sets username to whatever is in the username input everytime the input changes
    public void SetUsername(string _username)
    {
        username = _username;
    }
    
    // Sets email to whatever is in the email input everytime the input changes
    public void SetEmail(string _email)
    {
        email = _email;
    }
    
    // Sets password to whatever is in the password input everytime the input changes
    public void SetPassword(string _password)
    {
        password = _password;
    }

    // Sets remember to whatever the toggle is set too
    public void SetRemember(bool _remember)
    {
        remember = _remember;
    }

    // Called when registeration is successful
    void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        // Sets text to say "Registered & logged in!" and activates the username panel
        messageText.color = Color.white;
        messageText.text = "Registered & logged in!";
        UsernamePanel.SetActive(true);
        LoginPanel.SetActive(false);

        // If remember toggled save email & password to the users system
        if (remember)
        {
            PlayerPrefs.SetString("EMAIL", email);
            PlayerPrefs.SetString("PASSWORD", password);
        }
    }

    // Called when login is successful
    void OnLoginSuccess(LoginResult result)
    {
        // Sets text to say "Logged in!"
        messageText.color = Color.white;
        messageText.text = "Logged in!";

        // If remember toggled save email & password to the users system
        if (remember)
        {
            PlayerPrefs.SetString("EMAIL", email);
            PlayerPrefs.SetString("PASSWORD", password);
        }
        
        // Checks if a display name is set on PlayFab
        string name = null;
        if (result.InfoResultPayload.PlayerProfile != null)
            name = result.InfoResultPayload.PlayerProfile.DisplayName;

        // If no display name activate username panel
        if (name == null)
        {
            UsernamePanel.SetActive(true);
            LoginPanel.SetActive(false);
        }
        // If display name store the username on the users system and load the menu scene
        else
        {
            PlayerPrefs.SetString("USERNAME", name);
            LoadScene();
        }
    }

    // Called when guest login is successful
    void OnGuestLoginSuccess(LoginResult result)
    {
        // Sets text to say "Logged in!"
        messageText.color = Color.white;
        messageText.text = "Logged in";
        
        // Checks if a display name is set on PlayFab
        string name = null;
        if (result.InfoResultPayload.PlayerProfile != null)
            name = result.InfoResultPayload.PlayerProfile.DisplayName;

        // If no display name activate username panel
        if (name == null)
        {
            UsernamePanel.SetActive(true);
            LoginPanel.SetActive(false);
        }
        // If display name store the username on the users system and load the menu scene
        else
        {
            PlayerPrefs.SetString("USERNAME", name);
            LoadScene();
        }
    }

    // Called when display name update is successful
    void OnDisplayNameSuccess(UpdateUserTitleDisplayNameResult result)
    {
        Debug.Log($"Updated display name to {username}");
        
        // Stores username on the users system & loads the main menu scene
        PlayerPrefs.SetString("USERNAME", username);
        LoadScene();
    }

    // Called when password is reset
    void OnPasswordReset(SendAccountRecoveryEmailResult result)
    {
        messageText.color = Color.white;
        messageText.text = "Password reset mail sent!";
    }

    // Called whenever there is an error with PlayFab
    void OnError(PlayFabError error)
    {
        messageText.color = Color.red;
        messageText.text = $"Error: {error.GenerateErrorReport()}";
    }
    
    // Loads main menu scene
    void LoadScene()
    {
        SceneManager.LoadScene("Launcher");
    }
}
