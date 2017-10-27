using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Text.RegularExpressions;

public class Register : MonoBehaviour {
    public GameObject username;
    public GameObject password;
    public GameObject email;
    public GameObject confPassword;
    public GameObject completeName;
    public GameObject physioToggle;
    public GameObject labelr;

    private string Username;
    private string Password;
    private string Email;
    private string ConfPassword;
    private string CompleteName;
    private int PhysioToggle;
    private bool UsernameExists = false;
    private bool RegisterSuccessful = false;

    public static string ServerAddress = "ruvemji.atwebpages.com";

    private string form;
    private bool EmailValid = false;
    private string[] Characters = {"a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t","u","v","w","x","y","z",
                                   "A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V","W","X","Y","Z",
                                   "1","2","3","4","5","6","7","8","9","0","_","-"};

    void Start()
    {
        physioToggle.GetComponent<Toggle>().isOn = false; //Setea por default el toggle de "Physiotherapis" en 0, le quita el chulito
    }

	// Update is called once per frame
	void Update ()
    {   
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (username.GetComponent<InputField>().isFocused)
            {
                email.GetComponent<InputField>().Select();
            }
            if (email.GetComponent<InputField>().isFocused)
            {
                completeName.GetComponent<InputField>().Select();
            }
            if(completeName.GetComponent<InputField>().isFocused)
            {
                password.GetComponent<InputField>().Select();
            }
            if (password.GetComponent<InputField>().isFocused)
            {
                confPassword.GetComponent<InputField>().Select();
            }
        }

        if (username.GetComponent<InputField>().isFocused || confPassword.GetComponent<InputField>().isFocused || email.GetComponent<InputField>().isFocused || completeName.GetComponent<InputField>().isFocused || password.GetComponent<InputField>().isFocused)
        {
            labelr.GetComponent<Text>().text = "";
        }

        Username = username.GetComponent<InputField>().text;
        Email = email.GetComponent<InputField>().text;
        Password = password.GetComponent<InputField>().text;
        ConfPassword = confPassword.GetComponent<InputField>().text;
        CompleteName = completeName.GetComponent<InputField>().text;
        PhysioToggle = Convert.ToInt32(physioToggle.GetComponent<Toggle>().isOn);

    }

    public void RegisterBT()
    {
        StartCoroutine(SignUpCoroutine());
    }

    IEnumerator SignUpCoroutine() //Realiza el registro valiendose de las otras corrutinas
    {
        bool UN = false;
        bool EM = false;
        bool PW = false;
        bool CPW = false;

        WWWForm userformWWW = new WWWForm();
        userformWWW.AddField("Username", Username);
        WWW uservalidw = new WWW(ServerAddress + "/checkusername.php", userformWWW);
        yield return uservalidw;

        yield return StartCoroutine(UsernameValidCoroutine(uservalidw)); // vaya haga la corrutina y espere a que termine esa corrutina

        if (Username != "")
        {
            if (!UsernameExists)
            {
                UN = true;
            }
            else
            {
                Debug.LogWarning("Username Already Taken");
                labelr.GetComponent<Text>().text = "Username already exists";
            }
        }
        else
        {
            Debug.LogWarning("Username field empty");
            labelr.GetComponent<Text>().text = "Don't let any input field empty";
        }

        if (Email != "")
        {
            EmailValidation();
            if (EmailValid)
            {
                if (Email.Contains("@"))
                {
                    if (Email.Contains("."))
                    {
                        EM = true;
                    }
                    else
                    {
                        Debug.LogWarning("Email is Incorrect .");
                    }
                }
                else
                {
                    Debug.LogWarning("Email is Incorrect _");
                }
            }
            else
            {
                Debug.LogWarning("Email is Incorrect");
            }
        }
        else
        {
            Debug.LogWarning("Email Field Empty");
            labelr.GetComponent<Text>().text = "Don't let any input field empty";
        }
        if (Password != "")
        {
            if (Password.Length > 5)
            {
                PW = true;
            }
            else
            {
                Debug.LogWarning("Password must be at least 6 characters long");
                labelr.GetComponent<Text>().text = "Password must be at least 6 characters long";
            }
        }
        else
        {
            Debug.LogWarning("Password field is empty");
            labelr.GetComponent<Text>().text = "Don't let any input field empty";
        }
        if (ConfPassword != "")
        {
            if (ConfPassword == Password)
            {
                CPW = true;
            }
            else
            {
                Debug.LogWarning("Passwords don't match");
                labelr.GetComponent<Text>().text = "Passwords don't match";
            }
        }
        else
        {
            Debug.LogWarning("Conf. Password field is empty");
            labelr.GetComponent<Text>().text = "Don't let any input field empty";
        }



        if (UN)
        {
            if (CPW == true && PW == true && EM == true)
            {
                yield return StartCoroutine(DBRegisterCoroutine());// vaya haga la corrutina y espere a que termine esa corrutina

                if (RegisterSuccessful)
                {
                    username.GetComponent<InputField>().text = "";
                    email.GetComponent<InputField>().text = "";
                    password.GetComponent<InputField>().text = "";
                    confPassword.GetComponent<InputField>().text = "";
                    completeName.GetComponent<InputField>().text = "";

                    print("Registration Complete");
                }
                else
                {
                    username.GetComponent<InputField>().text = "";
                    email.GetComponent<InputField>().text = "";
                    password.GetComponent<InputField>().text = "";
                    confPassword.GetComponent<InputField>().text = "";
                    completeName.GetComponent<InputField>().text = "";

                    print("Registration Unsuccessful");
                }
            }
                
        }

    }

    IEnumerator UsernameValidCoroutine(WWW uservalidw) //Comprueba ya existe o no el usuario que digitó la persona
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

    IEnumerator DBRegisterCoroutine()
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

         WWWForm formWWW = new WWWForm();
         formWWW.AddField("Username", Username);
         formWWW.AddField("Password", Password);
         formWWW.AddField("CompleteName", CompleteName);
         formWWW.AddField("Email", Email);
         formWWW.AddField("PhysioToggle", PhysioToggle);
         WWW w = new WWW(ServerAddress + "/register.php", formWWW);
         yield return w;

         yield return StartCoroutine(registervalidationCoroutine(w));

         username.GetComponent<InputField>().text = "";
         email.GetComponent<InputField>().text = "";
         password.GetComponent<InputField>().text = "";
         confPassword.GetComponent<InputField>().text = "";
         completeName.GetComponent<InputField>().text = "";

         print("Registration Complete");

    } //Envía los datos del formulario al webservice register.php para agregarlos a la DB

    IEnumerator registervalidationCoroutine(WWW w) //Comprueba si la respuesta del servidor fue REGISTER-SUCCESSFUL
    {
        if (w.error == null)
        {
            Debug.LogWarning(w.text);
            if (w.text == "REGISTER-SUCCESSFUL")
            {
                RegisterSuccessful = true;
            }
            else
            {
                RegisterSuccessful = false;
            }
        }
        else
        {
            Debug.LogWarning("Error: " + w.error);
        }
        yield return RegisterSuccessful;
    }

    public void EmailValidation() //Mira si el email escrito parece ser un email (contiene un @, un ., empieza y termina en una letra
    {
        bool SW = false;
        bool EW = false;
        for (int i = 0; i < Characters.Length; i++)
        {
            if (Email.StartsWith(Characters[i]))
            {
                SW = true;
            }
        }
        for (int i = 0; i < Characters.Length; i++)
        {
            if (Email.EndsWith(Characters[i]))
            {
                EW = true;
            }
        }
        if (SW == true && EW == true)
        {
            EmailValid = true;
        }
        else
        {
            EmailValid = false;
        }
    }

 
}
