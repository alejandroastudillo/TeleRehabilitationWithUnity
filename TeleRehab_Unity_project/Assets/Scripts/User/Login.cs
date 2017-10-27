using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using System.Text.RegularExpressions;


public class Login : MonoBehaviour
{
    public GameObject username;
    public GameObject password;
    public GameObject labell;
    public GameObject PhysioConfPanel;
    public GameObject loginFather;
    public GameObject registerFather;
    public GameObject physiowelcome;
	public GameObject PanelSelectCOM;
	public GameObject PanelSelectPatient;
	public GameObject PanelSelectSession;
	public GameObject PanelSelectCOMPatient;
	public GameObject LoginPanel;
	public GameObject RegisterPanel;

    public static string Username;
    private string Password;
    private String[] Lines;
    private string DecryptedPass;

    private string ServerAddress = Register.ServerAddress;

    private bool UsernameExists = false;
    private bool LoginSuccessful = false;
    private bool isPhysiotherapist = false;

    void Start()
    {
        PhysioConfPanel.SetActive(false);
		PanelSelectCOM.SetActive(false);
		PanelSelectPatient.SetActive (false);
		PanelSelectSession.SetActive (false);
		PanelSelectCOMPatient.SetActive (false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (username.GetComponent<InputField>().isFocused)
            {
                password.GetComponent<InputField>().Select();
            }
        }

        if (username.GetComponent<InputField>().isFocused || password.GetComponent<InputField>().isFocused)
        {
            labell.GetComponent<Text>().text = "";
        }

        Username = username.GetComponent<InputField>().text;
        Password = password.GetComponent<InputField>().text;

    }

    public void LoginBT()
    {
        StartCoroutine(SignInCoroutine());
    }

    IEnumerator SignInCoroutine()
    {
        bool UN = false;
        bool PW = false;

        WWWForm userformWWW = new WWWForm();
        userformWWW.AddField("Username", Username);
        WWW uservalidw = new WWW(ServerAddress + "/checkusername.php", userformWWW);
        yield return uservalidw; //Espera a que la clase WWW retorne algo

        yield return StartCoroutine(UsernameValidCoroutine(uservalidw)); // vaya haga la corrutina y espere a que termine esa corrutina

        if (Username != "")
        {
            if (UsernameExists)
            {
                UN = true;
            }
            else
            {
                Debug.LogWarning("Username Incorrect UN");
                labell.GetComponent<Text>().text = "Username doesn't exist";
            }
        }
        else
        {
            Debug.LogWarning("Username field empty");
            labell.GetComponent<Text>().text = "Don't let any input field empty";
        }

        if (UN)
        {
            yield return StartCoroutine(PasswordValidCoroutine());// vaya haga la corrutina y espere a que termine esa corrutina

            if (LoginSuccessful)
            {
                PW = true;
            }
            else
            {
                Debug.LogWarning("Password is incorrect");
                labell.GetComponent<Text>().text = "Incorrect password";
            }
            if (UN == true && PW == true) //Si el usuario existe y la contraseña coincide en la DB
            {

                print("Login Sucessful");

                WWWForm userphWWW = new WWWForm();
                userphWWW.AddField("Username", Username);
                WWW physiow = new WWW(ServerAddress + "/isphysiotherapist.php", userphWWW);
                yield return physiow; //Espera a que la clase WWW retorne algo
                yield return StartCoroutine(IsPhysiotherapistCoroutine(physiow)); // vaya haga la corrutina y espere a que termine esa corrutina

                if (isPhysiotherapist)
                {
                    print( Username +" is a Physiotherapist");

                    PhysioConfPanel.SetActive(true);
					PanelSelectPatient.SetActive (true);
					LoginPanel.SetActive(false);
					RegisterPanel.SetActive(false);
					PanelSelectCOMPatient.SetActive(false);
					//UsersList.FillDropdown ();
                  
                    physiowelcome.GetComponent<Text>().text = "Welcome " + Username;
                    //SceneManager.LoadScene("PhysioConfiguration", LoadSceneMode.Single); //Cargar siguiente escena
                    //SceneManager.UnloadScene("LogIn");
                }
                else
                {
                    print("You are Pacient");
					//UsersList.patientsessioninfo.

					PhysioConfPanel.SetActive(true);
					PanelSelectCOMPatient.SetActive(true);

					UsersList.patientsessioninfo.Username = Username;

					COMSelectPatient comselectpatient = PanelSelectCOMPatient.GetComponent<COMSelectPatient> (); //Ejecuta la corrutina que busca las sesiones anteriores de un paciente
					comselectpatient.StartCoroutine (comselectpatient.GetCOMPORTSCoroutine());

					PanelSelectPatient.SetActive (false);
					LoginPanel.SetActive(false);
					RegisterPanel.SetActive(false);
                    //SceneManager.LoadScene("BodyMotion", LoadSceneMode.Single); //Cargar siguiente escena
                    //SceneManager.UnloadScene("LogIn");
                }
                username.GetComponent<InputField>().text = "";
                password.GetComponent<InputField>().text = "";
            }
        }



    }

    IEnumerator UsernameValidCoroutine(WWW uservalidw)
    {
        if (uservalidw.error == null)
        {
            Debug.LogWarning(uservalidw.text);
            if (uservalidw.text == "exists")
            {
                UsernameExists = true;
            }
            else
            {
                UsernameExists = false;
            }

        }
        else
        {
            Debug.LogWarning("Error: " + uservalidw.error);
        }
        yield return UsernameExists;

    }

    IEnumerator PasswordValidCoroutine()
    {
        bool Clear = true;
        int i = 1;
        foreach (char c in Password) //Encriptación
        {
            if (Clear)
            {
                Password = "";
                Clear = false;
            }
            i++;
            char Encrypted = (char)(c * i);
            Password += Encrypted.ToString();
        }
        WWWForm loginformWWW = new WWWForm();
        loginformWWW.AddField("Username", Username);
        loginformWWW.AddField("Password", Password);
        WWW loginw = new WWW(ServerAddress + "/login.php", loginformWWW);
        yield return loginw; //Espera a que la clase WWW retorne algo

        yield return StartCoroutine(LoginValidCoroutine(loginw));
    }

    IEnumerator LoginValidCoroutine(WWW loginw)
    {
        if (loginw.error == null)
        {
            Debug.LogWarning(loginw.text);
            if (loginw.text == "LOGIN-SUCCESSFUL")
            {
                LoginSuccessful = true;
            }
            else
            {
                LoginSuccessful = false;
            }

        }
        else
        {
            Debug.LogWarning("Error: " + loginw.error);
        }
        yield return LoginSuccessful;
    }

    IEnumerator IsPhysiotherapistCoroutine(WWW physiow)
    {
        if (physiow.error == null)
        {
            Debug.LogWarning(physiow.text);
            if (physiow.text == "PHYSIOTHERAPIST")
            {
                isPhysiotherapist = true;
            }
            else
            {
                isPhysiotherapist = false;
            }

        }
        else
        {
            Debug.LogWarning("Error: " + physiow.error);
        }

        yield return isPhysiotherapist;
    }

}
