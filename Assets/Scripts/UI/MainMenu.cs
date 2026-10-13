using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Menu principal : Jouer, choix du niveau, instructions, quitter.</summary>
public class MainMenu : MonoBehaviour
{
    public string level1Scene = "Level1";
    public string level2Scene = "Level2";

    GameObject instructionsPanel;

    void Start()
    {
        Time.timeScale = 1f;
        Transform canvas = UIFactory.CreateCanvas("Menu Canvas").transform;
        Vector2 left = new Vector2(0f, 0.5f), c = new Vector2(0.5f, 0.5f);

        UIFactory.CreateText(canvas, "Title", "ECHOBOT", 120, TextAnchor.MiddleLeft, UIFactory.Cyan, left, new Vector2(120f, 300f), new Vector2(900f, 150f), FontStyle.Bold, true);
        UIFactory.CreateText(canvas, "Subtitle", "Votre robot se souvient", 40, TextAnchor.MiddleLeft, Color.white, left, new Vector2(126f, 200f), new Vector2(900f, 60f), FontStyle.Italic, true);

        Vector2 size = new Vector2(420f, 76f);
        UIFactory.CreateButton(canvas, "►  JOUER", left, new Vector2(120f, 70f), size, () => SceneManager.LoadScene(level1Scene), new Color(0.05f, 0.3f, 0.25f, 0.95f), 32)
            .GetComponent<RectTransform>().pivot = new Vector2(0f, 0.5f);
        UIFactory.CreateButton(canvas, "NIVEAU 2 : L'USINE", left, new Vector2(120f, -25f), size, () => SceneManager.LoadScene(level2Scene))
            .GetComponent<RectTransform>().pivot = new Vector2(0f, 0.5f);
        UIFactory.CreateButton(canvas, "INSTRUCTIONS", left, new Vector2(120f, -120f), size, () => instructionsPanel.SetActive(true))
            .GetComponent<RectTransform>().pivot = new Vector2(0f, 0.5f);
        UIFactory.CreateButton(canvas, "QUITTER", left, new Vector2(120f, -215f), size, Quit, new Color(0.3f, 0.07f, 0.07f, 0.95f))
            .GetComponent<RectTransform>().pivot = new Vector2(0f, 0.5f);

        instructionsPanel = UIFactory.CreateFullscreen(canvas, "Instructions", new Color(0f, 0f, 0f, 0.75f)).gameObject;
        UIFactory.CreatePanel(instructionsPanel.transform, "Box", UIFactory.PanelColor, c, Vector2.zero, new Vector2(1180f, 740f));
        UIFactory.CreateText(instructionsPanel.transform, "Title", "INSTRUCTIONS", 54, TextAnchor.MiddleCenter, UIFactory.Cyan, c, new Vector2(0f, 300f), new Vector2(1100f, 80f), FontStyle.Bold);
        UIFactory.CreateText(instructionsPanel.transform, "Body",
            "Vous êtes un technicien piégé dans une usine robotique abandonnée.\n" +
            "Votre seul allié : EchoBot, un petit robot qui répète vos mouvements.\n\n" +
            "<b>ZQSD / WASD / flèches</b> : se déplacer\n" +
            "<b>Clic droit + souris</b> : tourner la caméra      <b>Molette</b> : zoom\n" +
            "<b>E</b> : utiliser une console ou un levier\n" +
            "<b>R</b> : enregistrer      <b>F</b> : arrêter      <b>G</b> : EchoBot rejoue\n" +
            "<b>Échap</b> : pause\n\n" +
            "Astuce : enregistrez un trajet jusqu'à une plaque, puis laissez EchoBot\n" +
            "le rejouer pendant que vous activez l'autre mécanisme !",
            26, TextAnchor.MiddleCenter, Color.white, c, new Vector2(0f, -10f), new Vector2(1100f, 520f));
        UIFactory.CreateButton(instructionsPanel.transform, "COMPRIS !", c, new Vector2(0f, -300f), new Vector2(320f, 64f), () => instructionsPanel.SetActive(false));
        instructionsPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && instructionsPanel != null) instructionsPanel.SetActive(false);
    }

    static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
