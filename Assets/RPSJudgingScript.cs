using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class RPSJudgingScript : MonoBehaviour
{
	public KMAudio Audio;
    public KMBombInfo Bomb;
    public KMBombModule Module;
	public KMBossModule Boss;
	
	public SpriteRenderer[] Renderers;
	public AudioClip[] MusicMaterial;
	public KMSelectable[] NumberButtons;
	public KMSelectable[] FlagResults;
	public KMSelectable[] Nothing;
	public KMSelectable Submit;
	public MeshRenderer CenterModule;
	public MeshRenderer Border;
	public Sprite[] SpriteLeft;
	public Sprite[] SpriteRight;
	public TextMesh Round;
	public TextMesh Winner;
	public Material[] Backgrounds;
	public TextMesh NumberItem;
	public GameObject Rewind;
	
	public GameObject[] TypingInput;
	
	int[] Cuprum = {0, 0};
	long[] Score = {0, 0};
	long RoundNumber = 0;
	string[] Results = {"Dynamite", "Tornado", "Quicksand", "Pit", "Chain", "Gun", "Law", "Whip", "Sword", "Rock", "Death", "Wall", "Sun", "Camera", "Fire", "Chainsaw", "School", "Scissors", "Poison", "Cage", "Axe", "Peace", "Computer", "Castle", "Snake", "Blood", "Porcupine", "Vulture", "Monkey", "King", "Queen", "Prince", "Princess", "Police", "Woman", "Baby", "Man", "Home", "Train", "Car", "Noise", "Bicycle", "Tree", "Turnip", "Duck", "Wolf", "Cat", "Bird", "Fish", "Spider", "Cockroach", "Brain", "Community", "Cross", "Money", "Vampire", "Sponge", "Church", "Butter", "Book", "Paper", "Cloud", "Airplane", "Moon", "Grass", "Film", "Toilet", "Air", "Planet", "Guitar", "Bowl", "Cup", "Beer", "Rain", "Water", "TV", "Rainbow", "UFO", "Alien", "Prayer", "Mountain", "Satan", "Dragon", "Diamond", "Platinum", "Gold", "Devil", "Fence", "Video Game", "Math", "Robot", "Heart", "Electricity", "Lightning", "Medusa", "Power", "Laser", "Nuke", "Sky", "Tank", "Helicopter"};
	
	int ActualStage = 0;
	int Team = 0;
	int MaxStage;
	bool Playable = false;
	bool Updatable = false;
	string[] ScoreLength = {"", ""};
	
	//Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool ModuleSolved;
	
	private string[] IgnoredModules;
	
	void Awake()
	{
		moduleId = moduleIdCounter++;
		for (int b = 0; b < NumberButtons.Count(); b++)
        {
            int Numbered = b;
            NumberButtons[Numbered].OnInteract += delegate
            {
                Press(Numbered);
				return false;
            };
        }
		for (int x = 0; x < Nothing.Count(); x++)
        {
            int Empty = x;
            Nothing[Empty].OnInteract += delegate
            {
                Useless(Empty);
				return false;
            };
        }
		for (int z = 0; z < FlagResults.Count(); z++)
        {
            int FlagNumeral = z;
            FlagResults[FlagNumeral].OnInteract += delegate
            {
                FlagPress(FlagNumeral);
				return false;
            };
        }
		Submit.OnInteract += delegate () { Submiting(); return false; };
	}

	void Start()
	{
		if (IgnoredModules == null)
            IgnoredModules = Boss.GetIgnoredModules("RPS Judging", new string[]{
				"14",
				"Brainf---",
				"Cookie Jars",
				"Divided Squares",
				"Forget Enigma",
				"Forget Everything",
				"Forget Infinity",
				"Forget It Not",
				"Forget Me Later",
				"Forget Me Not",
				"Forget Perspective",
				"Forget The Colors",
				"Forget Them All",
				"Forget This",
				"Forget Us Not",
				"Hogwarts",
				"Organization",
				"Purgatory",
				"RPS Judging",
				"Simon Forgets",
				"Simon's Stages",
				"Souvenir",
				"Tallordered Keys",
				"The Swan",
				"The Time Keeper",
				"The Troll",
				"The Very Annoying Button",
				"Timing is Everything",
				"Turn The Key",
				"Ultimate Custom Night",
				"Übermodule"
            });
		ClearFace();
		Module.OnActivate += StartingNumber;
	}
	
	void ClearFace()
	{
		for (int b = 0; b < 6; b++)
		{
			TypingInput[3+b].SetActive(false);
		}
		this.GetComponent<KMSelectable>().UpdateChildren();
	}
	
	void StartingNumber()
	{
		MaxStage = Bomb.GetSolvableModuleNames().Where(a => !IgnoredModules.Contains(a)).Count();
		Playable = true;
		Updatable = true;
		StartCoroutine(Playtime());
	}
	
	void Update()
	{
		if (ActualStage < Bomb.GetSolvedModuleNames().Where(a => !IgnoredModules.Contains(a)).Count() && !ModuleSolved)
        {
            ActualStage++;
        }
		
		if (MaxStage > 0 && ActualStage == MaxStage && Updatable && Playable)
		{
			Updatable = false;
			StopAllCoroutines();
			StartCoroutine(Whistling());
		}
	}
	
	void ScoreProcessing()
	{
		CenterModule.material = Backgrounds[1];
		for (int x = 0; x < 3; x++)
		{
			TypingInput[x].SetActive(false);
		}
		for (int y = 0; y < 3; y++)
		{
			TypingInput[3+y].SetActive(true);
		}
		this.GetComponent<KMSelectable>().UpdateChildren();
	}
	
	void Submiting()
	{
		Submit.AddInteractionPunch(.2f);
		Audio.PlaySoundAtTransform(MusicMaterial[4].name, transform);
		ScoreLength[Team] = NumberItem.text;
		if (Team == 0)
		{
			Debug.LogFormat("[RPS Judging #{0}] You provided the info: Blue Team: {1}", moduleId, ScoreLength[0]);
			CenterModule.material = Backgrounds[2];
			NumberItem.text = "";
			Team++;
		}
		else
		{
			Debug.LogFormat("[RPS Judging #{0}] You provided the info: Red Team: {1}", moduleId, ScoreLength[1]);
			CenterModule.material = Backgrounds[3];
			NumberItem.text = "";
			for (int y = 0; y < 3; y++)
			{
				TypingInput[3+y].SetActive(false);
			}
			for (int x = 0; x < 3; x++)
			{
				TypingInput[6+x].SetActive(true);
			}
			this.GetComponent<KMSelectable>().UpdateChildren();
		}
	}
	
	void Useless (int Empty)
	{
		Nothing[Empty].AddInteractionPunch(.2f);
	}
	
	void Press (int Numbered)
	{
		NumberButtons[Numbered].AddInteractionPunch(.2f);
		Audio.PlaySoundAtTransform(MusicMaterial[3].name, transform);
		if (NumberItem.text.Length < 12)
		{
			NumberItem.text += Numbered.ToString();
		}
	}
	
	void FlagPress (int FlagNumeral)
	{
		FlagResults[FlagNumeral].AddInteractionPunch(.2f);
		if (FlagNumeral == 0)
		{
			Debug.LogFormat("[RPS Judging #{0}] You declared the winner is the Blue Team", moduleId);
			if (ScoreLength[0].Length > 0 && ScoreLength[1].Length > 0)
			{
				if ((ScoreLength[0].Length > 1 && ScoreLength[0][0].ToString() != "0") || (ScoreLength[1].Length > 1 && ScoreLength[1][0].ToString() != "0"))
				{
					if ((long.Parse(ScoreLength[0]) == Score[0]) && (long.Parse(ScoreLength[1]) == Score[1]))
					{
						if (long.Parse(ScoreLength[0]) > long.Parse(ScoreLength[1]))
						{
							for (int x = 0; x < 3; x++)
							{
								TypingInput[6+x].SetActive(false);
							}
							Debug.LogFormat("[RPS Judging #{0}] You have provided correct informations. Module solved!", moduleId);
							Border.material = Backgrounds[1];
							CenterModule.material = Backgrounds[4];
							Audio.PlaySoundAtTransform(MusicMaterial[5].name, transform);
							Winner.text = "Blue Team\nWins!";
							Module.HandlePass();
						}
						
						else
						{
							StartCoroutine(Mistake());
						}
					}
					
					else
					{
						StartCoroutine(Mistake());
					}
				}
				
				else if (ScoreLength[0].Length == 1 && ScoreLength[1].Length == 1)
				{
					if ((long.Parse(ScoreLength[0]) == Score[0]) && (long.Parse(ScoreLength[1]) == Score[1]))
					{
						if (long.Parse(ScoreLength[0]) > long.Parse(ScoreLength[1]))
						{
							for (int x = 0; x < 3; x++)
							{
								TypingInput[6+x].SetActive(false);
							}
							Debug.LogFormat("[RPS Judging #{0}] You have provided correct informations. Module solved!", moduleId);
							Border.material = Backgrounds[1];
							CenterModule.material = Backgrounds[4];
							Audio.PlaySoundAtTransform(MusicMaterial[5].name, transform);
							Winner.text = "Blue Team\nWins!";
							Module.HandlePass();
						}
						
						else
						{
							StartCoroutine(Mistake());
						}
					}
					
					else
					{
						StartCoroutine(Mistake());
					}
				}
				
				else
				{
					StartCoroutine(Mistake());
				}
			}
			
			else
			{
				StartCoroutine(Mistake());
			}
		}
		
		if (FlagNumeral == 1)
		{
			Debug.LogFormat("[RPS Judging #{0}] You declared the winner is the Red Team", moduleId);
			if (ScoreLength[0].Length > 0 && ScoreLength[1].Length > 0)
			{
				if ((ScoreLength[0].Length > 1 && ScoreLength[0][0].ToString() != "0") || (ScoreLength[1].Length > 1 && ScoreLength[1][0].ToString() != "0"))
				{
					if ((long.Parse(ScoreLength[0]) == Score[0]) && (long.Parse(ScoreLength[1]) == Score[1]))
					{
						if (long.Parse(ScoreLength[0]) < long.Parse(ScoreLength[1]))
						{
							for (int x = 0; x < 3; x++)
							{
								TypingInput[6+x].SetActive(false);
							}
							Debug.LogFormat("[RPS Judging #{0}] You have provided correct informations. Module solved!", moduleId);
							Border.material = Backgrounds[2];
							CenterModule.material = Backgrounds[4];
							Audio.PlaySoundAtTransform(MusicMaterial[5].name, transform);
							Winner.text = "Red Team\nWins!";
							Module.HandlePass();
						}
						
						else
						{
							StartCoroutine(Mistake());
						}
					}
					
					else
					{
						StartCoroutine(Mistake());
					}
				}
				
				else if (ScoreLength[0].Length == 1 && ScoreLength[1].Length == 1)
				{
					if ((long.Parse(ScoreLength[0]) == Score[0]) && (long.Parse(ScoreLength[1]) == Score[1]))
					{
						if (long.Parse(ScoreLength[0]) < long.Parse(ScoreLength[1]))
						{
							for (int x = 0; x < 3; x++)
							{
								TypingInput[6+x].SetActive(false);
							}
							Debug.LogFormat("[RPS Judging #{0}] You have provided correct informations. Module solved!", moduleId);
							Border.material = Backgrounds[2];
							CenterModule.material = Backgrounds[4];
							Audio.PlaySoundAtTransform(MusicMaterial[5].name, transform);
							Winner.text = "Red Team\nWins!";
							Module.HandlePass();
						}
						
						else
						{
							StartCoroutine(Mistake());
						}
					}
					
					else
					{
						StartCoroutine(Mistake());
					}
				}
				
				else
				{
					StartCoroutine(Mistake());
				}
			}
			
			else
			{
				StartCoroutine(Mistake());
			}
		}
		
		if (FlagNumeral == 2)
		{
			Debug.LogFormat("[RPS Judging #{0}] You declared a draw", moduleId);
			if (ScoreLength[0].Length > 0 && ScoreLength[1].Length > 0)
			{
				if ((ScoreLength[0].Length > 1 && ScoreLength[0][0].ToString() != "0") || (ScoreLength[1].Length > 1 && ScoreLength[1][0].ToString() != "0"))
				{
					if ((long.Parse(ScoreLength[0]) == Score[0]) && (long.Parse(ScoreLength[1]) == Score[1]))
					{
						if (long.Parse(ScoreLength[0]) == long.Parse(ScoreLength[1]))
						{
							for (int x = 0; x < 3; x++)
							{
								TypingInput[6+x].SetActive(false);
							}
							Debug.LogFormat("[RPS Judging #{0}] You have provided correct informations. Module solved!", moduleId);
							Border.material = Backgrounds[3];
							CenterModule.material = Backgrounds[4];
							Audio.PlaySoundAtTransform(MusicMaterial[6].name, transform);
							Winner.text = "It Is A\nTie!";
							Module.HandlePass();
						}
						
						else
						{
							StartCoroutine(Mistake());
						}
					}
					
					else
					{
						StartCoroutine(Mistake());
					}
				}
				
				else if (ScoreLength[0].Length == 1 && ScoreLength[1].Length == 1)
				{
					if ((long.Parse(ScoreLength[0]) == Score[0]) && (long.Parse(ScoreLength[1]) == Score[1]))
					{
						if (long.Parse(ScoreLength[0]) == long.Parse(ScoreLength[1]))
						{
							for (int x = 0; x < 3; x++)
							{
								TypingInput[6+x].SetActive(false);
							}
							Debug.LogFormat("[RPS Judging #{0}] You have provided correct informations. Module solved!", moduleId);
							Border.material = Backgrounds[3];
							CenterModule.material = Backgrounds[4];
							Audio.PlaySoundAtTransform(MusicMaterial[6].name, transform);
							Winner.text = "It Is A\nTie!";
							Module.HandlePass();
						}
						
						else
						{
							StartCoroutine(Mistake());
						}
					}
					
					else
					{
						StartCoroutine(Mistake());
					}
				}
				
				else
				{
					StartCoroutine(Mistake());
				}
			}
			
			else
			{
				StartCoroutine(Mistake());
			}
		}
	}
	
	IEnumerator Playtime()
	{
		if (!ModuleSolved && Playable)
		{
			if (ActualStage == MaxStage || RoundNumber == 999999999999)
			{
				if (MaxStage > 0 || RoundNumber == 999999999999)
				{
					Playable = false;
					StartCoroutine(Whistling());
				}
				
				else
				{
					StartCoroutine(Solving());
				}
			}
			
			else
			{
				Audio.PlaySoundAtTransform(MusicMaterial[0].name, transform);
				for (int a = 0; a < 2; a++)
				{
					Cuprum[a] = UnityEngine.Random.Range(0,101);
				}
				
				Renderers[0].sprite = SpriteLeft[Cuprum[0]];
				Renderers[1].sprite = SpriteRight[Cuprum[1]];
				RoundNumber++;
				Round.text = "Round " + RoundNumber.ToString();
				
				bool Chill = false;
				for (int b = 0; b < 50; b++)
				{
					int Suplement = (Cuprum[0] + (b + 1)) % 101;
					if (Suplement == Cuprum[1])
					{
						Chill = true;
						break;
					}
				}
				
				Debug.LogFormat("[RPS Judging #{0}] Round {1}: {2} vs {3}", moduleId, RoundNumber.ToString(), Results[Cuprum[0]], Results[Cuprum[1]]);
				
				if (Cuprum[0] == Cuprum[1])
				{
					Debug.LogFormat("[RPS Judging #{0}] Its a draw!", moduleId);
				}
				
				else if (Chill == true)
				{
					Score[0]++;
					Debug.LogFormat("[RPS Judging #{0}] {1} beats {2}. Blue Team gets a point!", moduleId, Results[Cuprum[0]], Results[Cuprum[1]]);
				}
				
				else
				{
					Score[1]++;
					Debug.LogFormat("[RPS Judging #{0}] {1} loses to {2}. Red Team gets a point!", moduleId, Results[Cuprum[0]], Results[Cuprum[1]]);
				}
				Debug.LogFormat("[RPS Judging #{0}] ", moduleId);
				Playable = true;
				yield return new WaitForSecondsRealtime(30f);
				StartCoroutine(Playtime());
			}
		}
	}
	
	IEnumerator Solving()
	{
		yield return new WaitForSecondsRealtime(.001f);
		Debug.LogFormat("[RPS Judging #{0}] The module was instantly solved. Have a heart!}", moduleId);
		Playable = false;
		Module.HandlePass();
		ModuleSolved = true;
		Renderers[0].sprite = SpriteLeft[91];
		Renderers[1].sprite = SpriteRight[91];
		Round.text = "Module Solved";
		Audio.PlaySoundAtTransform(MusicMaterial[10].name, transform);
	}
	
	IEnumerator Whistling()
	{
		Debug.LogFormat("[RPS Judging #{0}] The game is ended! These are the following results: Blue: {1} / Red {2}", moduleId, Score[0].ToString(), Score[1].ToString());
		Debug.LogFormat("[RPS Judging #{0}] ", moduleId);
		Audio.PlaySoundAtTransform(MusicMaterial[1].name, transform);
		Round.text = "";
		yield return new WaitForSecondsRealtime(2.2f);
		Round.text = "LETS WRAP THIS UP!";
		Audio.PlaySoundAtTransform(MusicMaterial[7].name, transform);
		yield return new WaitForSecondsRealtime(5f);
		Audio.PlaySoundAtTransform(MusicMaterial[2].name, transform);
		ScoreProcessing();
	}
	
	IEnumerator Mistake()
	{
		for (int x = 0; x < 3; x++)
		{
			TypingInput[6+x].SetActive(false);
		}
		Audio.PlaySoundAtTransform(MusicMaterial[8].name, transform);
		yield return new WaitForSecondsRealtime(1f);
		Winner.text = "Wait!";
		Audio.PlaySoundAtTransform(MusicMaterial[7].name, transform);
		yield return new WaitForSecondsRealtime(1f);
		Winner.text = "Something\nIs Not\nRight";
		Audio.PlaySoundAtTransform(MusicMaterial[7].name, transform);
		yield return new WaitForSecondsRealtime(1f);
		Winner.text = "Fix It,\nPlease!";
		Audio.PlaySoundAtTransform(MusicMaterial[7].name, transform);
		yield return new WaitForSecondsRealtime(1f);
		Winner.text = "Thank You!";
		Audio.PlaySoundAtTransform(MusicMaterial[7].name, transform);
		yield return new WaitForSecondsRealtime(1f);
		Winner.text = "Now,\nGo Back!";
		Audio.PlaySoundAtTransform(MusicMaterial[7].name, transform);
		yield return new WaitForSecondsRealtime(1f);
		Rewind.SetActive(true);
		Winner.text = "";
		Audio.PlaySoundAtTransform(MusicMaterial[9].name, transform);
		yield return new WaitForSecondsRealtime(2.7f);
		Rewind.SetActive(false);
		ScoreLength[0] = ""; ScoreLength[1] = "";
		Team = 0;
		Debug.LogFormat("[RPS Judging #{0}] You provided an incorrect information. A strike has been given.", moduleId);
		Debug.LogFormat("[RPS Judging #{0}] ", moduleId);
		Module.HandleStrike();
		ScoreProcessing();
	}
}
