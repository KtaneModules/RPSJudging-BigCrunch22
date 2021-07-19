using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

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
	public KMSelectable[] RecoveryButtons;
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

	List<int> LeftDisplays = new List<int>();
	List<int> RightDisplays = new List<int>();
	int[] Cuprum = {0, 0};
	long[] Score = {0, 0};
	long RoundNumber = 0;
	string[] Results = {"Dynamite", "Tornado", "Quicksand", "Pit", "Chain", "Gun", "Law", "Whip", "Sword", "Rock", "Death", "Wall", "Sun", "Camera", "Fire", "Chainsaw", "School", "Scissors", "Poison", "Cage", "Axe", "Peace", "Computer", "Castle", "Snake", "Blood", "Porcupine", "Vulture", "Monkey", "King", "Queen", "Prince", "Princess", "Police", "Woman", "Baby", "Man", "Home", "Train", "Car", "Noise", "Bicycle", "Tree", "Turnip", "Duck", "Wolf", "Cat", "Bird", "Fish", "Spider", "Cockroach", "Brain", "Community", "Cross", "Money", "Vampire", "Sponge", "Church", "Butter", "Book", "Paper", "Cloud", "Airplane", "Moon", "Grass", "Film", "Toilet", "Air", "Planet", "Guitar", "Bowl", "Cup", "Beer", "Rain", "Water", "TV", "Rainbow", "UFO", "Alien", "Prayer", "Mountain", "Satan", "Dragon", "Diamond", "Platinum", "Gold", "Devil", "Fence", "Video Game", "Math", "Robot", "Heart", "Electricity", "Lightning", "Medusa", "Power", "Laser", "Nuke", "Sky", "Tank", "Helicopter"};
	
	int ActualStage = 0;
	int Team = 0;
	int RecoverIndex = 0;
	int MaxStage;
	bool Playable = false;
	//bool Updatable = false;
	bool InRecovery = false;
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
		for (int x = 0; x < RecoveryButtons.Count(); x++)
        {
            int Btn = x;
			RecoveryButtons[Btn].OnInteract += delegate
            {
                RecoverPress(Btn);
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
		Submit.OnInteract += delegate () { Submitting(); return false; };
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
		//Updatable = true;
		StartCoroutine(Playtime());
	}
	
	void Update()
	{
		if (ActualStage < Bomb.GetSolvedModuleNames().Where(a => !IgnoredModules.Contains(a)).Count() && !ModuleSolved)
        {
            ActualStage++;
			StartCoroutine(Playtime());
        }
		
		//if (MaxStage > 0 && ActualStage == MaxStage && Updatable && Playable)
		//{
		//	Updatable = false;
		//	StopAllCoroutines();
		//	StartCoroutine(Whistling());
		//}
	}
	
	void ScoreProcessing()
	{
		PlaytimeIsOver = true;
		Scoreboard = true;
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

	void EnterRecovery()
    {
		InRecovery = true;
		Audio.PlaySoundAtTransform(MusicMaterial[0].name, transform);
		CenterModule.material = Backgrounds[0];
		for (int y = 0; y < 3; y++)
		{
			TypingInput[3+y].SetActive(false);
		}
		for (int x = 0; x < 3; x++)
		{
			TypingInput[x].SetActive(true);
		}
		RecoverIndex = 0;
		Renderers[0].sprite = SpriteLeft[LeftDisplays[RecoverIndex]];
		Renderers[1].sprite = SpriteRight[RightDisplays[RecoverIndex]];
		Round.text = "Round " + (RecoverIndex + 1).ToString();
		this.GetComponent<KMSelectable>().UpdateChildren();
	}
	
	void Submitting()
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
			Scoreboard = false;
		}
	}
	
	void RecoverPress (int Btn)
	{
		RecoveryButtons[Btn].AddInteractionPunch(.2f);
		if (Btn == 0 && InRecovery && RecoverIndex != 0)
        {
			Audio.PlaySoundAtTransform(MusicMaterial[0].name, transform);
			RecoverIndex--;
			Renderers[0].sprite = SpriteLeft[LeftDisplays[RecoverIndex]];
			Renderers[1].sprite = SpriteRight[RightDisplays[RecoverIndex]];
			Round.text = "Round " + (RecoverIndex + 1).ToString();
		}
		else if (Btn == 1 && InRecovery && RecoverIndex != (MaxStage - 1))
		{
			Audio.PlaySoundAtTransform(MusicMaterial[0].name, transform);
			RecoverIndex++;
			Renderers[0].sprite = SpriteLeft[LeftDisplays[RecoverIndex]];
			Renderers[1].sprite = SpriteRight[RightDisplays[RecoverIndex]];
			Round.text = "Round " + (RecoverIndex + 1).ToString();
		}
		else if (Btn == 2 && InRecovery)
		{
			InRecovery = false;
			Audio.PlaySoundAtTransform(MusicMaterial[2].name, transform);
			ScoreProcessing();
		}
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
							ModuleSolved = true;
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
							ModuleSolved = true;
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
							ModuleSolved = true;
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
							ModuleSolved = true;
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
							ModuleSolved = true;
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
							ModuleSolved = true;
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
				LeftDisplays.Add(Cuprum[0]);
				RightDisplays.Add(Cuprum[1]);

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
				yield return new WaitForSecondsRealtime(0f);
				//StartCoroutine(Playtime());
			}
		}
	}
	
	IEnumerator Solving()
	{
		yield return new WaitForSecondsRealtime(.001f);
		Debug.LogFormat("[RPS Judging #{0}] The module was instantly solved. Have a heart!", moduleId);
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
		if (Score[0] > Score[1])
		{
			Debug.LogFormat("[RPS Judging #{0}] Blue Team wins!", moduleId);
		}
		else if (Score[0] < Score[1])
		{
			Debug.LogFormat("[RPS Judging #{0}] Red Team wins!", moduleId);
		}
		else
		{
			Debug.LogFormat("[RPS Judging #{0}] Its a draw!", moduleId);
		}
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
		Scolded = true;
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
		Scolded = false;
		EnterRecovery();
	}
	
	//twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"To submit the score on the current scoreboard, do !{0} submit [score] | To decide the result of the battle, do !{0} flag [color] | To review a specific round when in review mode, do !{0} review [round] | To exit review mode, do !{0} continue | Colors that are valid are: red, blue, and gray";
    #pragma warning restore 414
	
	bool PlaytimeIsOver = false;
	bool Scoreboard = false;
	bool Scolded = false;
	string[] Validity = {"0", "1", "2", "3", "4", "5", "6", "7", "8", "9"};

	IEnumerator ProcessTwitchCommand(string command)
	{
		if (Regex.IsMatch(command, @"^\s*continue\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			yield return null;
			if (PlaytimeIsOver == false)
			{
				yield return "sendtochaterror The game is still active. The command was not processed.";
				yield break;
			}

			else if (Scolded == true)
			{
				yield return "sendtochaterror The referee is telling you that a mistake has occured. The command was not processed.";
				yield break;
			}

			else if (InRecovery == false)
			{
				yield return "sendtochaterror You are currently not in review mode at this instance. The command was not processed.";
				yield break;
			}

			RecoveryButtons[2].OnInteract();
		}
		string[] parameters = command.Split(' ');
		if (Regex.IsMatch(parameters[0], @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			yield return null;
			if (parameters.Length > 2 || parameters.Length < 2)
			{
				yield return "sendtochaterror Parameter length is invalid. The command was not processed.";
				yield break;
			}

			else if (PlaytimeIsOver == false)
			{
				yield return "sendtochaterror The game is still active. The command was not processed.";
				yield break;
			}

			else if (Scolded == true)
			{
				yield return "sendtochaterror The referee is telling you that a mistake has occured. The command was not processed.";
				yield break;
			}

			else if (Scoreboard == false)
			{
				yield return "sendtochaterror You are currently not in the scoring board at this instance. The command was not processed.";
				yield break;
			}

			else if (parameters.Length == 2)
			{
				foreach (char c in parameters[1])
				{
					if (!c.ToString().EqualsAny(Validity))
					{
						yield return "sendtochaterror The score being submitted contains a character that is not a number. The command was not processed.";
						yield break;
					}
				}

				if (parameters[1].Length > 12)
				{
					yield return "sendtochaterror The number that was given is longer than 12 digits. The command was not processed.";
					yield break;
				}

				foreach (char c in parameters[1])
				{
					NumberButtons[Int32.Parse(c.ToString())].OnInteract();
					yield return new WaitForSecondsRealtime(0.1f);
				}
				Submit.OnInteract();
			}
		}

		if (Regex.IsMatch(parameters[0], @"^\s*flag\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			if (parameters.Length > 2 || parameters.Length < 2)
			{
				yield return "sendtochaterror Parameter length is invalid. The command was not processed.";
				yield break;
			}

			yield return null;
			if (PlaytimeIsOver == false)
			{
				yield return "sendtochaterror The game is still active. The command was not processed.";
				yield break;
			}

			else if (Scolded == true)
			{
				yield return "sendtochaterror The referee is telling you that a mistake has occured. The command was not processed.";
				yield break;
			}

			else if (Scoreboard == true || InRecovery == true)
			{
				yield return "sendtochaterror You are currently not deciding the winner in this instance. The command was not processed.";
				yield break;
			}
			
			else
			{
				if (Regex.IsMatch(parameters[1], @"^\s*red\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(parameters[1], @"^\s*r\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
				{
					yield return "strike";
					FlagResults[1].OnInteract();
				}
				
				else if (Regex.IsMatch(parameters[1], @"^\s*blue\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(parameters[1], @"^\s*b\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
				{
					yield return "strike";
					FlagResults[0].OnInteract();
				}
				
				else if (Regex.IsMatch(parameters[1], @"^\s*gray\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(parameters[1], @"^\s*grey\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(parameters[1], @"^\s*g\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
				{
					yield return "strike";
					FlagResults[2].OnInteract();
				}
				
				else
				{
					yield return "sendtochaterror The color of the flag that was sent was not valid. The command was not processed.";
				}
			}
		}

		if (Regex.IsMatch(parameters[0], @"^\s*review\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			yield return null;
			if (parameters.Length > 2 || parameters.Length < 2)
			{
				yield return "sendtochaterror Parameter length is invalid. The command was not processed.";
				yield break;
			}

			else if (PlaytimeIsOver == false)
			{
				yield return "sendtochaterror The game is still active. The command was not processed.";
				yield break;
			}

			else if (Scolded == true)
			{
				yield return "sendtochaterror The referee is telling you that a mistake has occured. The command was not processed.";
				yield break;
			}

			else if (InRecovery == false)
			{
				yield return "sendtochaterror You are currently not in review mode at this instance. The command was not processed.";
				yield break;
			}

			else if (parameters.Length == 2)
			{
				foreach (char c in parameters[1])
				{
					if (!c.ToString().EqualsAny(Validity))
					{
						yield return "sendtochaterror The round being submitted contains a character that is not a number. The command was not processed.";
						yield break;
					}
				}

				long round = long.Parse(parameters[1]);
				if (round < 1 || round > MaxStage)
				{
					yield return "sendtochaterror The number that was given is not in the range of 1-"+MaxStage+". The command was not processed.";
					yield break;
				}

				if (RecoverIndex < (round - 1))
                {
					while (RecoverIndex != (round - 1))
                    {
						RecoveryButtons[1].OnInteract();
						yield return new WaitForSecondsRealtime(0.05f);
					}
                }
				else
				{
					while (RecoverIndex != (round - 1))
					{
						RecoveryButtons[0].OnInteract();
						yield return new WaitForSecondsRealtime(0.05f);
					}
				}
			}
		}
	}

	IEnumerator TwitchHandleForcedSolve()
    {
		if (Scolded)
        {
			HandleTPSolverPass();
			yield break;
        }
		if ((Team == 1 && !Score[0].ToString().Equals(ScoreLength[0])) || (PlaytimeIsOver && !Scoreboard && !InRecovery && !Score[1].ToString().Equals(ScoreLength[1])))
        {
			HandleTPSolverPass();
			yield break;
		}
		if (Scoreboard)
        {
			string scoreString = Score[Team].ToString();
			if (NumberItem.text.Length > scoreString.Length)
            {
				HandleTPSolverPass();
				yield break;
			}
			for (int i = 0; i < NumberItem.text.Length; i++)
            {
				if (!NumberItem.text[i].Equals(scoreString[i]))
                {
					HandleTPSolverPass();
					yield break;
				}
            }
        }
		while (!PlaytimeIsOver) yield return true;
		if (InRecovery)
        {
			RecoveryButtons[2].OnInteract();
			yield return new WaitForSecondsRealtime(0.1f);
		}
		if (Scoreboard)
        {
			int start = Team;
			for (int i = start; i < 2; i++)
            {
				int start2 = NumberItem.text.Length;
				for (int j = start2; j < Score[i].ToString().Length; j++)
				{
					NumberButtons[Int32.Parse(Score[i].ToString()[j].ToString())].OnInteract();
					yield return new WaitForSecondsRealtime(0.1f);
				}
				Submit.OnInteract();
				yield return new WaitForSecondsRealtime(0.1f);
			}
		}
		if (Score[0] > Score[1])
			FlagResults[0].OnInteract();
		else if (Score[0] < Score[1])
			FlagResults[1].OnInteract();
		else
			FlagResults[2].OnInteract();
	}

	void HandleTPSolverPass()
    {
		StopAllCoroutines();
		CenterModule.material = Backgrounds[4];
		if (Rewind.activeSelf)
			Rewind.SetActive(false);
		for (int y = 0; y < 6; y++)
		{
			TypingInput[3+y].SetActive(false);
		}
		if (Score[0] > Score[1])
		{
			Border.material = Backgrounds[1];
			Audio.PlaySoundAtTransform(MusicMaterial[5].name, transform);
			Winner.text = "Blue Team\nWins!";
		}
		else if (Score[0] < Score[1])
		{
			Border.material = Backgrounds[2];
			Audio.PlaySoundAtTransform(MusicMaterial[5].name, transform);
			Winner.text = "Red Team\nWins!";
		}
		else
		{
			Border.material = Backgrounds[3];
			Audio.PlaySoundAtTransform(MusicMaterial[6].name, transform);
			Winner.text = "It Is A\nTie!";
		}
		Module.HandlePass();
	}
}
