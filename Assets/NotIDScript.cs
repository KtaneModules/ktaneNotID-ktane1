﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class NotIDScript : MonoBehaviour
{
	public KMAudio audio;
    public KMBombInfo bomb;
    public KMBombModule module;
	
	public KMSelectable[] TypableLetters;
	public KMSelectable[] TypableNumbers;
	public KMSelectable[] TypableSymbols;
	public KMSelectable[] ShiftButtons;
	public KMSelectable[] UselessButtons;
	public KMSelectable Backspace;
	public KMSelectable Enter;
	public KMSelectable SpaceBar;

	public MeshRenderer moduleBorder;
	public MeshRenderer moduleSurface;
	public GameObject border;
	public Sprite[] borderSprite;
	public GameObject[] screen;
	public GameObject[] logo;
	public Material[] borderMat;
	public Material[] surfaceMat;
	private int idModel;
	
	public MeshRenderer[] LightBulbs;
	public Material[] TheLights;
	
	public TextMesh[] Text;
	public TextMesh TextBox;
	public GameObject TheBox;

	public KMSelectable display;
	
	bool Shifted = false;
		
	string[][] ChangedText = new string[2][]{
		new string[47] {"`", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "-", "=", "q", "w", "e", "r", "t", "y", "u", "i", "o", "p", "[", "]", "\\", "a", "s", "d", "f", "g", "h", "j", "k", "l", ";", "'", "z", "x", "c", "v", "b", "n", "m", ",", ".", "/"},
		new string[47] {"~", "!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "_", "+", "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P", "{", "}", "|", "A", "S", "D", "F", "G", "H", "J", "K", "L", ":", "\"", "Z", "X", "C", "V", "B", "N", "M", "<", ">", "?"}
	};

	private KeyCode[] LetterKeys =
	{
		KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T, KeyCode.Y, KeyCode.U, KeyCode.I, KeyCode.O, KeyCode.P,
		KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.J, KeyCode.K, KeyCode.L,
		KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V, KeyCode.B, KeyCode.N, KeyCode.M
	};

	private KeyCode[] NumberKeys =
	{
		KeyCode.Alpha0, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9 
	};
	
	private KeyCode[] SymbolKeys =
	{
		KeyCode.BackQuote, KeyCode.Minus, KeyCode.Equals,
		KeyCode.LeftBracket, KeyCode.RightBracket, KeyCode.Backslash,
		KeyCode.Semicolon, KeyCode.Quote,
		KeyCode.Comma, KeyCode.Period, KeyCode.Slash,
	};

	private KeyCode[] ShiftKeys =
	{
		KeyCode.LeftShift, KeyCode.RightShift
	};

	private KeyCode[] UselessKeys =
	{
		KeyCode.Tab, KeyCode.CapsLock, KeyCode.LeftControl, KeyCode.LeftWindows,  KeyCode.LeftAlt, KeyCode.RightAlt, KeyCode.RightWindows, KeyCode.Menu, KeyCode.RightControl
	};

	private KeyCode[] OtherKeys =
	{
		KeyCode.Backspace, KeyCode.Return, KeyCode.Space
	};
	private bool focused;

	private string[] finalAnswer = new string[4];

	//Stage 1 things
	private string[] keyboardLayouts = new string[] { "QWERTYUIOPASDFGHJKLZXCVBNM", "AOEUIDHTNSQJKXBMWVZPYFGCRL", "ARSTDHNEIOQWFPGJLUYZXCVBKM", "ASHTGYNEOIQDRWBJFUPZXMCVKL", "ASDTGHNIOEQWPRFYUKLXZCVBJM", "ASETGYNIOHQWDFKJURLZXCVBPM" };
	private string[] keyboardNames = new string[] { "QWERTY", "DVORAK", "COLEMAK", "WORKMAN", "QWPR", "NORMAN" };
	private static string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
	private int selectedKeyboard;
	private string finalKeyboardLayout;

	//Stage 2 things
	private int goalNumber;
	private string numberClues;

	//Stage 3 things
	private string[] initialSymbolString = new string[32] { "`", "-", "=", "[", "]", "\\", ";", "'", ",", ".", "/", "!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "~", "_", "+", "{", "}", "|", ":", "\"", "<", ">", "?" };
	private string[] finalSymbolString = new string[32];

	//Stage 4 things
	private StringBuilder pressedUselessKeys = new StringBuilder();
	string[] uselessNames = new string[9] { "Tab", "Caps Lock", "Left Ctrl", "Left Win", "Left Alt", "Right Alt", "Right Win", "Menu", "Right Ctrl" };

	bool Enterable = true;
	int Stages = 0;

	bool isAnimating;

	//Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool ModuleSolved;

	void Awake()
	{
		moduleId = moduleIdCounter++;

		display.OnInteract += () => { doNothing(); return false; };

		for (int b = 0; b < TypableLetters.Count(); b++)
        {
            int KeyPress = b;
            TypableLetters[KeyPress].OnInteract += delegate
            {
                TypeLetter(KeyPress);
				return false;
            };
        }

		for (int b = 0; b < TypableNumbers.Count(); b++)
		{
			int KeyPress = b;
			TypableNumbers[KeyPress].OnInteract += delegate
			{
				TypeNumber(KeyPress);
				return false;
			};
		}

		for (int b = 0; b < TypableSymbols.Count(); b++)
		{
			int KeyPress = b;
			TypableSymbols[KeyPress].OnInteract += delegate
			{
				TypeSymbol(KeyPress);
				return false;
			};
		}

		for (int a = 0; a < ShiftButtons.Count(); a++)
        {
            int Shifting = a;
            ShiftButtons[Shifting].OnInteract += delegate
            {
                PressShift(Shifting);
				return false;
            };
        }
		
		for (int c = 0; c < UselessButtons.Count(); c++)
        {
            int Useless = c;
            UselessButtons[Useless].OnInteract += delegate
            {
				TypeUseless(Useless);
				return false;
            };
        }
		
		Backspace.OnInteract += delegate () { PressBackspace(); return false; };
		Enter.OnInteract += delegate () { PressEnter(); return false; };
		SpaceBar.OnInteract += delegate () { PressSpaceBar(); return false; };
		GetComponent<KMSelectable>().OnFocus += delegate () { focused = true; };
		GetComponent<KMSelectable>().OnDefocus += delegate () { focused = false; };
		if (Application.isEditor)
			focused = true;
	}
	
	
	void Start()
	{
		this.GetComponent<KMSelectable>().UpdateChildren();
		idModel = UnityEngine.Random.Range(0, 8);
		moduleBorder.material = borderMat[idModel];
		moduleSurface.material = surfaceMat[idModel];
		for (int i = 0; i < screen.Length; i++)
        {
			if (i == idModel)
            {
				screen[i].SetActive(true);
				logo[i].SetActive(true);
            }
            else
            {
				screen[i].SetActive(false);
				logo[i].SetActive(false);
			}
        }
		if (idModel == 1) { screen[1].SetActive(true); }
		if (idModel == 2)
        {
			border.GetComponent<SpriteRenderer>().sprite = borderSprite[1];
			border.transform.localScale = new Vector3(0.07f, 0.07f, 0.07f);
        }
        else
        {
			border.GetComponent<SpriteRenderer>().sprite = borderSprite[0];
			border.transform.localScale = new Vector3(0.03039575f, 0.03039575f, 0.03039575f);
		}
		module.OnActivate += Introduction;
	}
	
	void Introduction()
	{
		string idModName = "";
		switch (idModel)
        {
			case 0:
				idModName = "Piece Identification";
				break;
			case 1:
				idModName = "VTuber Identification";
				break;
			case 2:
				idModName = "Touhou Identification";
				break;
			case 3:
				idModName = "FNF Identification";
				break;
			case 4:
				idModName = "Magisword Identification";
				break;
			case 5:
				idModName = "Stand Identification";
				break;
			case 6:
				idModName = "Animatronic Identification";
				break;
			case 7:
				idModName = "Item Identification";
				break;
        }
		Debug.LogFormat("[Not Identification #{0}] Module initiated, and it takes the form of {1}.", moduleId, idModName);
		StageOne();
		Enterable = true;
		
	}
	
	void StageOne()
    {
		selectedKeyboard = UnityEngine.Random.Range(1, keyboardLayouts.Length);
		int caesarOffset = UnityEngine.Random.Range(0, keyboardLayouts[selectedKeyboard].Length);
		StringBuilder sb = new StringBuilder();
		StringBuilder sb2 = new StringBuilder();
		for (int i = 0; i < keyboardLayouts[selectedKeyboard].Length; i++)
        {
			sb.Append(alphabet[(keyboardLayouts[selectedKeyboard][i] + caesarOffset - 65) % 26]);
			if (sb[i] == keyboardLayouts[0][i] && sb2.Length < 8)
            {
				sb2.Append(sb[i]);
            }
		}
		finalKeyboardLayout = sb.ToString();
		Debug.LogFormat("[Not Identification #{0}] Stage 1: Chosen keyboard layout is {1}, with Caesar offset {2}.", moduleId, keyboardNames[selectedKeyboard], caesarOffset);
		Debug.LogFormat("<Not Identification #{0}> Stage 1: The full keyboard is {1}.", moduleId, finalKeyboardLayout);
		finalAnswer[0] = (sb2.ToString()).ToLowerInvariant() + keyboardNames[selectedKeyboard];
		if (finalAnswer[0].Length % 2 != 0)
		{
			finalAnswer[0] += (alphabet[(caesarOffset + 25) % 26].ToString()).ToLowerInvariant();
		}
        else
        {
			finalAnswer[0] += (alphabet[(caesarOffset + 25) % 26].ToString()).ToUpperInvariant();
		}
		Debug.LogFormat("[Not Identification #{0}] Stage 1: The correct string to submit is {1}.", moduleId, finalAnswer[0]);
	}

	void StageTwo()
    {
		goalNumber = UnityEngine.Random.Range(100, 1000);
		for (int a = 0; a < LightBulbs.Length; a++)
        {
			if (a == goalNumber % 3) { LightBulbs[a].material = TheLights[2]; }
            else { LightBulbs[a].material = TheLights[0]; }
        }
		HashSet<int> zeroToFour = new HashSet<int>();
		int[] shuffler = new int[5];
		int i = goalNumber;
		while (i > 0)
        {
			zeroToFour.Add(i % 10);
			i = i / 10;
		}
		while (zeroToFour.Count < 5)
        {
			zeroToFour.Add(UnityEngine.Random.Range(0, 10));
        }
		zeroToFour.CopyTo(shuffler);
		shuffler.Shuffle();
		for (int j = 0; j < 10; j++)
        {
			if (j < 5)
            {
				numberClues += shuffler[j].ToString();
            }
            else
            {
				numberClues += (goalNumber % j).ToString();
            }
        }
		finalAnswer[1] = goalNumber.ToString();
		Debug.LogFormat("[Not Identification #{0}] Stage 2: Light bulb #{1} is lit up red.", moduleId, goalNumber % 3 + 1);
		Debug.LogFormat("[Not Identification #{0}] Stage 2: The numbers 0 to 4 types out {1}, {2}, {3}, {4}, {5} respectively.", moduleId, numberClues[0], numberClues[1], numberClues[2], numberClues[3], numberClues[4]);
		Debug.LogFormat("[Not Identification #{0}] Stage 2: The numbers 5 to 9 types out {1}, {2}, {3}, {4}, {5} respectively.", moduleId, numberClues[5], numberClues[6], numberClues[7], numberClues[8], numberClues[9]);
		Debug.LogFormat("[Not Identification #{0}] Stage 2: The expected goal number is {1}, although the module accepts any valid three-digit solution.", moduleId, goalNumber);
	}

	void StageThree()
    {
		int keyNumber = 0;
		for (int i = 0; i < LightBulbs.Length; i++)
        {
			int rnd = UnityEngine.Random.Range(0, TheLights.Length - 1);
			LightBulbs[i].material = TheLights[rnd];
			keyNumber += rnd;
			switch (rnd)
            {
				case 0:
					Debug.LogFormat("[Not Identification #{0}] Stage 3: Light bulb #{1} is unlit.", moduleId, i);
					break;
				case 1:
					Debug.LogFormat("[Not Identification #{0}] Stage 3: Light bulb #{1} is lit up green.", moduleId, i);
					break;
				case 2:
					Debug.LogFormat("[Not Identification #{0}] Stage 3: Light bulb #{1} is lit up red.", moduleId, i);
					break;
				case 3:
					Debug.LogFormat("[Not Identification #{0}] Stage 3: Light bulb #{1} is lit up blue.", moduleId, i);
					break;
			}
		}
		string[] mainLoop = initialSymbolString.ToArray();
		mainLoop.Shuffle();
		for (int i = 0; i < mainLoop.Length; i++)
        {
			for (int j = 0; j < initialSymbolString.Length; j++)
            {
				if (initialSymbolString[j] == mainLoop[i])
                {
					if (i == mainLoop.Length - 1)
					{
						finalSymbolString[j] = mainLoop[0];
					}
                    else
                    {
						finalSymbolString[j] = mainLoop[i + 1];
					}
				}
			}
        }
		StringBuilder sb = new StringBuilder();
		string c = finalSymbolString[keyNumber + 10];
		int q = 0;
		while (sb.Length < 10)
        {
			if (c == initialSymbolString[q])
			{
				sb.Append(c);
				c = finalSymbolString[q];
			}
			q++;
			if (q >= finalSymbolString.Length) { q = 0; }
		}

		finalAnswer[2] = sb.ToString();
		Debug.LogFormat("[Not Identification #{0}] Stage 3: The light bulb values total up to {1}.", moduleId, keyNumber);
		Debug.LogFormat("[Not Identification #{0}] Stage 3: The final string obtained should be {1}", moduleId, sb.ToString());
	}

	void StageFour()
    {
		int num = UnityEngine.Random.Range(0, 8);
		Debug.LogFormat("[Not Identification #{0}] Stage 4: The light bulbs form the number {1}.", moduleId, num);
		int pair = 0;
		string binary = Convert.ToInt32(Convert.ToString(num, 2)).ToString("000");
		StringBuilder sb = new StringBuilder();
		for (int i = 0; i < LightBulbs.Length; i++)
        {
			if (binary[i] == '0') { LightBulbs[i].material = TheLights[0]; }
            else { LightBulbs[i].material = TheLights[4]; }
        }
		for (int i = 0; i < 2; i++)
        {
			if (i == 0)
			{
				sb.Append(num);
			}
            else
            {
				num = 8 - num;
				while ((sb.ToString()).Contains((num).ToString()))
				{
					num--;
					num = (num % 9 + 9) % 9;
				}
				sb.Append(num);
			}
			switch (num)
			{
				case 2:
					pair = 8;
					break;
				case 3:
					pair = 6;
					break;
				case 4:
					pair = 5;
					break;
				case 5:
					pair = 4;
					break;
				case 6:
					pair = 3;
					break;
				case 8:
					pair = 2;
					break;
				default:
					break;
			}
			if (pair != 0)
            {
				while ((sb.ToString()).Contains(pair.ToString()))
				{
					pair++;
					pair = (pair % 9 + 9) % 9;
				}
				sb.Append(pair); pair = 0;
			}
		}
		finalAnswer[3] = sb.ToString();
		sb.Remove(0, sb.Length);
		for (int i = 0; i < finalAnswer[3].Length; i++)
        {
			sb.Append(uselessNames[finalAnswer[3][i] - '0'] + ", ");
        }
		sb.Remove(sb.Length - 2, 2);
		Debug.LogFormat("[Not Identification #{0}] Stage 4: The first few button presses should be {1}.", moduleId, sb.ToString());
	}

	void AnswerChecker()
    {
		switch (Stages)
        {
			case 0:
			case 2:
				if (TextBox.text == finalAnswer[Stages])
                {
					Stages++;
					Debug.LogFormat("[Not Identification #{0}] Input {1} matches with answer {2}, that is correct! Advancing stage...", moduleId, TextBox.text, finalAnswer[Stages]);
				}
                else
                {
					Debug.LogFormat("[Not Identification #{0}] Input {1} does not match with answer {2}, strike.", moduleId, TextBox.text, finalAnswer[Stages]);
					module.HandleStrike();
					TextBox.text = "";
					return;
				}
				break;
			case 1:
				bool isAccepted = true;
				if (TextBox.text == finalAnswer[Stages])
                {
					isAccepted = true;
                }
                else
                {
					int k = 0;
					bool c = int.TryParse(TextBox.text, out k);
					if (!c) { isAccepted = false; }
                    else if (k < 100 || k > 999) { isAccepted = false; }
					else if (LightBulbs[k % 3].material != TheLights[2]) { isAccepted = false; }
					else
                    {
						for (int i = 5; i < 10; i++)
                        {
							if (k % i != numberClues[i]) { isAccepted = false; }
                        }
                    }
				}
				if (isAccepted == true)
                {
					Stages++;
					Debug.LogFormat("[Not Identification #{0}] Input {1} is an acceptable answer! Advancing stage...", moduleId, TextBox.text);
				}
                else
                {
					Debug.LogFormat("[Not Identification #{0}] Input {1} is not an acceptable answer, strike.", moduleId, TextBox.text);
					module.HandleStrike();
					TextBox.text = "";
					return;
				}
				break;
			case 3:
				int[] uselessCheckers = new int[9];
				bool passable = true;
				for (int i = 0; i < pressedUselessKeys.Length; i++)
                {
					uselessCheckers[pressedUselessKeys.ToString()[i] - '0']++;
				}
				for (int i = 0; i < uselessCheckers.Length; i++)
                {
					if (uselessCheckers[i] != 1) { passable = false; }
                }
				if (!passable)
                {
					Debug.LogFormat("[Not Identification #{0}] Each button is not pressed only once, strike.", moduleId); 
					module.HandleStrike(); 
					pressedUselessKeys.Remove(0, pressedUselessKeys.Length);
					return;
				}
				else if (finalAnswer[3] != pressedUselessKeys.ToString().Substring(0, finalAnswer[3].Length))
                {
					Debug.LogFormat("[Not Identification #{0}] The first few presses is wrong, strike.", moduleId);
					module.HandleStrike();
					pressedUselessKeys.Remove(0, pressedUselessKeys.Length);
					return;
				}
                else
                {
					Debug.LogFormat("[Not Identification #{0}] The input is acceptable! Advancing stage...", moduleId);
					Stages++;
				}
				break;
		}
		TextBox.text = "";
		StartCoroutine(AdvanceStage());
    }

	void TypeLetter(int k)
    {
		if (isAnimating) { return; }
		TypableLetters[k].AddInteractionPunch(.2f);
		audio.PlaySoundAtTransform("Keyboard Press", transform);
		if (Enterable)
        {
			if (Stages != 0)
            {
				module.HandleStrike();
				Debug.LogFormat("[Not Identification #{0}] A letter key is pressed in an incorrect stage, strike.", moduleId);
				return;
			}
			float width = 0;
			foreach (char symbol in TextBox.text)
			{
				CharacterInfo info;
				if (TextBox.font.GetCharacterInfo(symbol, out info, TextBox.fontSize, TextBox.fontStyle))
				{
					width += info.advance;
				}
			}
			width = width * TextBox.characterSize * 0.1f;

			if (width < 0.28f)
			{
				if (Shifted)
				{
					TextBox.text += (finalKeyboardLayout[k].ToString()).ToUpperInvariant();
				}
                else
                {
					TextBox.text += (finalKeyboardLayout[k].ToString()).ToLowerInvariant();
				}
				if (width > 0.28)
				{
					string Copper = TextBox.text;
					Copper = Copper.Remove(Copper.Length - 1);
					TextBox.text = Copper;
				}
			}
		}
	}

	void TypeNumber(int k)
	{
		if (isAnimating) { return; }
		TypableNumbers[k].AddInteractionPunch(.2f);
		audio.PlaySoundAtTransform("Keyboard Press", transform);
		if (Enterable)
		{
			float width = 0;
			foreach (char symbol in TextBox.text)
			{
				CharacterInfo info;
				if (TextBox.font.GetCharacterInfo(symbol, out info, TextBox.fontSize, TextBox.fontStyle))
				{
					width += info.advance;
				}
			}
			width = width * TextBox.characterSize * 0.1f;
			if (width < 0.28f)
			{
				if (!Shifted)
                {
					if (Stages != 1)
					{
						module.HandleStrike();
						Debug.LogFormat("[Not Identification #{0}] A number key is pressed in an incorrect stage, strike.", moduleId);
						return;
					}
					TextBox.text += numberClues[k].ToString();
				}
                else
                {
					if (Stages != 2)
					{
						module.HandleStrike();
						Debug.LogFormat("[Not Identification #{0}] A symbol key is pressed in an incorrect stage, strike.", moduleId);
						return;
					}
					if (k == 0)
						TextBox.text += finalSymbolString[20];
					else
						TextBox.text += finalSymbolString[k + 10];

				}
				if (width > 0.28)
				{
					string Copper = TextBox.text;
					Copper = Copper.Remove(Copper.Length - 1);
					TextBox.text = Copper;
				}
			}
		}
	}

	void TypeSymbol(int k)
	{
		if (isAnimating) { return; }
		TypableSymbols[k].AddInteractionPunch(.2f);
		audio.PlaySoundAtTransform("Keyboard Press", transform);
		if (Enterable)
		{
			if (Stages != 2)
			{
				module.HandleStrike();
				Debug.LogFormat("[Not Identification #{0}] A symbol key is pressed in an incorrect stage, strike.", moduleId);
				return;
			}
			float width = 0;
			foreach (char symbol in TextBox.text)
			{
				CharacterInfo info;
				if (TextBox.font.GetCharacterInfo(symbol, out info, TextBox.fontSize, TextBox.fontStyle))
				{
					width += info.advance;
				}
			}
			width = width * TextBox.characterSize * 0.1f;
			if (width < 0.28f)
			{
				if (Shifted)
                {
					TextBox.text += finalSymbolString[k + 21];
                }
                else
                {
					TextBox.text += finalSymbolString[k];
				}
				if (width > 0.28)
				{
					string Copper = TextBox.text;
					Copper = Copper.Remove(Copper.Length - 1);
					TextBox.text = Copper;
				}
			}
		}
	}

	void TypeUseless(int k)
    {
		if (isAnimating) { return; }
		UselessButtons[k].AddInteractionPunch(.2f);
		audio.PlaySoundAtTransform("Keyboard Press", transform);
		if (Stages != 3)
        {
			module.HandleStrike();
			Debug.LogFormat("[Not Identification #{0}] A useless key is pressed in an incorrect stage, strike.", moduleId);
			return;
		}
		pressedUselessKeys.Append(k);
		Debug.LogFormat("<Not Identification #{0}> Stage 4: {1} is pressed.", moduleId, uselessNames[k]);

	}

	void PressBackspace()
	{
		if (isAnimating) { return; }
		Backspace.AddInteractionPunch(.2f);
		audio.PlaySoundAtTransform("Keyboard Press", transform);
        if (Enterable)
		{
			if (TextBox.text.Length != 0)
			{
				string Copper = TextBox.text;
				Copper = Copper.Remove(Copper.Length - 1);
				TextBox.text = Copper;
			}
			else if (Stages == 3 && pressedUselessKeys.Length != 0)
            {
				pressedUselessKeys.Remove(0, pressedUselessKeys.Length);
				Debug.LogFormat("<Not Identification #{0}> Stage 4: Backspace pressed, all inputs cleared.", moduleId);
			}
		}
	}
	
	void PressSpaceBar()
	{
		SpaceBar.AddInteractionPunch(.2f);
		audio.PlaySoundAtTransform("Keyboard Press", transform);
        if (Enterable)
		{
			float width = 0;
			foreach (char symbol in TextBox.text)
			{
				CharacterInfo info;
				if (TextBox.font.GetCharacterInfo(symbol, out info, TextBox.fontSize, TextBox.fontStyle))
				{
					width += info.advance;
				}
			}
			width =  width * TextBox.characterSize * 0.1f;
			
			if (width < 0.28f)
			{
				TextBox.text += " ";
				if (width > 0.28)
				{
					string Copper = TextBox.text;
					Copper = Copper.Remove(Copper.Length - 1);
					TextBox.text = Copper;
				}
			}
		}
	}

	
	void PressEnter()
	{
		if (isAnimating) { return; }
		Enter.AddInteractionPunch(.2f);
		audio.PlaySoundAtTransform("Keyboard Press", transform);
		AnswerChecker();
	}
	
	void PressShift(int Shifting)
	{
		ShiftButtons[Shifting].AddInteractionPunch(.2f);
		audio.PlaySoundAtTransform("Keyboard Press", transform);
		Shifted = !Shifted;
		if (Shifted == true)
		{
			for (int b = 0; b < Text.Count(); b++)
			{
				Text[b].text = ChangedText[1][b];
			}
		}
		else
		{
			for (int a = 0; a < Text.Count(); a++)
			{
				Text[a].text = ChangedText[0][a];
			}
		}
	}

	IEnumerator AdvanceStage()
    {
		isAnimating = true;
		foreach (MeshRenderer i in LightBulbs)
        {
			i.material = TheLights[0];
        }
		switch (Stages)
        {
			case 1:
				audio.PlaySoundAtTransform("Stage", transform);
				break;
			case 2:
				audio.PlaySoundAtTransform("Stage2", transform);
				break;
			case 3:
				audio.PlaySoundAtTransform("Stage3", transform);
				break;
			default:
				break;
		}
		if (Stages < 4)
        {
			foreach (MeshRenderer i in LightBulbs)
			{
				i.material = TheLights[1];
				yield return new WaitForSeconds(0.216666666667f);
			}
		}
        else
        {
			ModuleSolved = true;
			audio.PlaySoundAtTransform("Solve1", transform);
			foreach (MeshRenderer i in LightBulbs)
			{
				i.material = TheLights[3];
				yield return new WaitForSeconds(0.216666666667f);
			}
			audio.PlaySoundAtTransform("Solve2", transform);
			foreach (MeshRenderer i in LightBulbs)
			{
				i.material = TheLights[4];
				yield return new WaitForSeconds(0.216666666667f);
			}
			audio.PlaySoundAtTransform("Solve3", transform);
			foreach (MeshRenderer i in LightBulbs)
			{
				i.material = TheLights[1];
				yield return new WaitForSeconds(0.216666666667f);
			}
		}
		if (Stages >= 4)
		{
			Debug.LogFormat("[Not Identification #{0}] All four stages are done, module solved!", moduleId);
			module.HandlePass();
		}
		else
		{
			switch (Stages)
			{
				case 1:
					StageTwo();
					break;
				case 2:
					StageThree();
					break;
				case 3:
					StageFour();
					break;
			}
		}
		isAnimating = false;
		yield return null;
    }

	void doNothing()
    {
		int rnd = UnityEngine.Random.Range(0, 5);
		switch (rnd)
        {
			case 0:
				Debug.LogFormat("<Not Identification #{0}> Nice try, but this ain't your normal identification mod.", moduleId);
				break;
			case 1:
				Debug.LogFormat("<Not Identification #{0}> Module activated, wait...whoops, force of habit.", moduleId);
				break;
			case 2:
				Debug.LogFormat("<Not Identification #{0}> Fun fact, these logs are for fixing a bug, so yeah it's not really that useless.", moduleId);
				break;
			case 3:
				Debug.LogFormat("<Not Identification #{0}> I refuse to be an ID mod. Go solve the module like intended in the manual you buffoon.", moduleId);
				break;
			case 4:
				Debug.LogFormat("<Not Identification #{0}> This does nothing! ", moduleId);
				break;
		}
	}

	void Update()//Runs every frame.
	{
		if (focused)
		{
			for (int i = 0; i < LetterKeys.Count(); i++)
			{
				if (Input.GetKeyDown(LetterKeys[i]))
				{
					TypableLetters[i].OnInteract();
				}
			}
			for (int i = 0; i < NumberKeys.Count(); i++)
			{
				if (Input.GetKeyDown(NumberKeys[i]))
				{
					TypableNumbers[i].OnInteract();
				}
			}
			for (int i = 0; i < SymbolKeys.Count(); i++)
			{
				if (Input.GetKeyDown(SymbolKeys[i]))
				{
					TypableSymbols[i].OnInteract();
				}
			}
			for (int j = 0; j < ShiftKeys.Count(); j++)
			{
				if (Input.GetKeyDown(ShiftKeys[j]))
				{
					ShiftButtons[j].OnInteract();
				}
			}
			for (int k = 0; k < UselessKeys.Count(); k++)
			{
				if (Input.GetKeyDown(UselessKeys[k]))
				{
					if (Stages == 3)
                    {
						UselessButtons[k].OnInteract();
					}
				}
			}
			for (int l = 0; l < OtherKeys.Count(); l++)
			{
				if (Input.GetKeyDown(OtherKeys[l]))
				{
					switch (l)
					{
						case 0:
							Backspace.OnInteract(); break;
						case 1:
							Enter.OnInteract(); break;
						case 2:
							SpaceBar.OnInteract(); break;
						default:
							break;
					}
				}
			}
		}
	}

#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"To type letters/digits/symbols, use <!{0} type [SOMETHING]>, to type useless keys, use <!{0} [KEY NAME] [KEY NAME]>, valid key names are Tab, Caps, Menu, LeftCtrl, RightCtrl, LeftWin, RightWin, LeftAlt, RightAlt, use <!{0} clear> to clear the text box or press the backspace button in stage 4, use <!{0} enter> to submit an answer";
#pragma warning restore 414

	IEnumerator ProcessTwitchCommand(string command)
    {
		yield return null;
		List<KMSelectable> presses = new List<KMSelectable>();
		if (Regex.IsMatch(command, @"^\s*start\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
			yield return "sendtochat Unable to identify the module, please try again."; yield break;
        }
		else if (Regex.IsMatch(command, @"^\s*j\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)) { yield return "sendtochat j"; yield break; }
		else if (Regex.IsMatch(command, @"^\s*ktane1\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)) { yield return "sendtochat Congratulations, now you get the privileges to ping ktane1 as long as you send along a screenshot of this message!"; yield break; }
		else if (Regex.IsMatch(command, @"^\s*clear\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
			if (Stages != 3 && TextBox.text.Length == 0) { yield return "sendtochat The module is extremely confused as there is literally nothing to clear in the text box."; yield break; }
			while (TextBox.text.Length != 0)
			{
				Backspace.OnInteract();
				yield return new WaitForSecondsRealtime(0.02f);
			}
			Backspace.OnInteract();
			yield return null;
			yield break;
		}
		else if (Regex.IsMatch(command, @"^\s*enter\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
			Enter.OnInteract();
			yield return null;
			if (ModuleSolved) { yield return "sendtochat PogChamp PogChamp PogChamp";}
			yield break;
        }
		string[] parameters = command.Split(' ');
		string current = "";
		bool TPshift = Shifted;
		if (Regex.IsMatch(parameters[0], @"^\s*type\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
			if (parameters.Length > 2) { yield return "sendtochat Spacebar detected, which, somehow, is the only useless key in the keyboard. The module is now visibly more depressed than before. Don't do that again."; yield break; }
			else if (parameters.Length < 2) { yield return "sendtochat Go on...type something..."; yield break; }
			else
            {
				foreach (char c in parameters[1])
				{
					if (!c.ToString().EqualsAny(ChangedText[0]) && !c.ToString().EqualsAny(ChangedText[1]))
					{
						yield return "sendtochat The module is extremely confused/scared on what exactly do you want to type. Give it a break, come on.";
						yield break;
					}
                    else
                    {
						current = TextBox.text;
						if (!(c - '0' < 0 || c - '0' > 9))
						{
							if (TPshift) { presses.Add(ShiftButtons[0]); TPshift = false; }
							presses.Add(TypableNumbers[c - '0']);
						}
                        else
                        {
							for (int i = 0; i < keyboardLayouts[0].Length; i++)
							{
								if (c == keyboardLayouts[0].ToUpperInvariant()[i])
                                {
									if (!TPshift) { presses.Add(ShiftButtons[0]); TPshift = true; }
									presses.Add(TypableLetters[i]);
								}
								else if (c == keyboardLayouts[0].ToLowerInvariant()[i])
								{
									if (TPshift) { presses.Add(ShiftButtons[0]); TPshift = false; }
									presses.Add(TypableLetters[i]);
								}
							}
							for (int i = 0; i < initialSymbolString.Length; i++)
							{
								if (c == initialSymbolString[i][0])
								{
									if (i < 11)
									{
										if (TPshift) { presses.Add(ShiftButtons[0]); TPshift = false; }
										presses.Add(TypableSymbols[i]);
									}
									else if (i > 20)
									{
										if (!TPshift) { presses.Add(ShiftButtons[0]); TPshift = true; }
										presses.Add(TypableSymbols[i - 21]);
									}
                                    else
                                    {
										if (!TPshift) { presses.Add(ShiftButtons[0]); TPshift = true; }
										if (c == ')') { presses.Add(TypableNumbers[0]); }
                                        else { presses.Add(TypableNumbers[i - 10]); }
									}
								}
							}
						}
					}
				}
				foreach (KMSelectable i in presses)
                {
					i.OnInteract();
					yield return new WaitForSeconds(0.05f);
				}
				yield return null;
			}
        }
		else
        {
			foreach (string lmao in parameters)
            {
				switch (lmao)
                {
					case "Tab":
						presses.Add(UselessButtons[0]);
						break;
					case "Caps":
					case "CapsLock":
						presses.Add(UselessButtons[1]);
						break;
					case "LeftCtrl":
						presses.Add(UselessButtons[2]);
						break;
					case "RightCtrl":
						presses.Add(UselessButtons[8]);
						break;
					case "LeftWin":
						presses.Add(UselessButtons[3]);
						break;
					case "RightWin":
						presses.Add(UselessButtons[6]);
						break;
					case "LeftAlt":
						presses.Add(UselessButtons[4]);
						break;
					case "RightAlt":
						presses.Add(UselessButtons[5]);
						break;
					case "Menu":
						presses.Add(UselessButtons[7]);
						break;
					default:
						yield return "sendtochat Please send a command that is more intelligible. The module is trying its best here :("; 
						yield break;
				}
            }
			foreach (KMSelectable i in presses)
			{
				i.OnInteract();
				yield return new WaitForSeconds(0.05f);
			}
			yield return null;
		}
	}

    IEnumerator TwitchHandleForcedSolve()
	{
        while (!ModuleSolved)
		{
			if (isAnimating) { yield return null; }
			else
			{
				while (TextBox.text != "")
				{
					Backspace.OnInteract();
					yield return null;
				}
				switch (Stages)
				{
					case 0:
						for (int i = 0; i < finalAnswer[0].Length; i++)
						{
							if ((finalAnswer[0][i] < 91 && !Shifted) || (finalAnswer[0][i] > 96 && Shifted))
							{
								ShiftButtons[0].OnInteract();
								yield return null;
							}
							TypableLetters[finalKeyboardLayout.IndexOf(Convert.ToChar(finalAnswer[0][i].ToString().ToUpper()))].OnInteract();
							yield return null;
						}
						Enter.OnInteract();
						break;
					case 1:
						int digit = 0;
						if (Shifted) { ShiftButtons[0].OnInteract(); yield return null; }
						for (int i = 0; i < finalAnswer[1].Length; i++)
						{
							TypableNumbers[numberClues.IndexOf(finalAnswer[1][i])].OnInteract();
							yield return null;
                        }
                        Enter.OnInteract();
                        break;
					case 2:
						int symbolToPress = 0;
						for (int i = 0; i < finalAnswer[2].Length; i++)
						{
							if (i == 0)
							{
								symbolToPress = Array.IndexOf(finalSymbolString, finalAnswer[2][i].ToString());
							}
							else
							{
								symbolToPress = Array.IndexOf(initialSymbolString, finalAnswer[2][i - 1].ToString());
							}
							if (symbolToPress < 11)
							{
                                if (Shifted) { ShiftButtons[0].OnInteract(); yield return null; }
								TypableSymbols[symbolToPress].OnInteract(); yield return null;
                            }
							else if (symbolToPress > 20)
							{
								if (!Shifted) { ShiftButtons[0].OnInteract(); yield return null; }
                                TypableSymbols[symbolToPress - 21].OnInteract(); yield return null;
                            }
							else
							{
                                if (!Shifted) { ShiftButtons[0].OnInteract(); yield return null; }
                                if (symbolToPress == 20) { TypableNumbers[0].OnInteract(); yield return null; }
								else { TypableNumbers[symbolToPress - 10].OnInteract(); yield return null; }
                            }
						}
						Enter.OnInteract();
						break;
					case 3:
						var f = new List<int>(){ 0, 1, 2, 3, 4, 5, 6, 7, 8 };
						for (int i = 0; i < finalAnswer[3].Length; i++)
						{
							UselessButtons[finalAnswer[3][i] - '0'].OnInteract();
							yield return null;
							f.Remove(finalAnswer[3][i] - '0');
						}
						for (int i = 0; i < f.Count; i++)
						{
							UselessButtons[f[i]].OnInteract();
							yield return null;
						}
						Enter.OnInteract();
                        break;
				}
				yield return null;
			}
		}


    }
}
