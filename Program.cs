using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AiHangman
{
    public class Program
    {
        

        static void Main(string[] args)
        {

            //Variable Initialisation
            int wordLength=0, maxWordLength = 29, maxGuesses=0, inputNum;
            string fileName = "dictionary.txt";
            bool printRemainingWords=false, validationBoolean = true, playAgain=true;

            List<string> fileWords = new List<string>();

            //Play game Loop
            while (playAgain)
            {
                //Word Length Input Validation and File reading
                while (wordLength <= 0 || wordLength > maxWordLength)
                {
                    wordLength = InputNum("the desired word length");
                    Console.WriteLine("Input of " + wordLength);

                    fileWords = ReadDictionary(fileName, wordLength);

                    Console.WriteLine(fileName + " read containing " + fileWords.Count + " word(s).");

                    if (fileWords.Count <= 0)
                    {
                        wordLength = 0;
                        Console.WriteLine("No words of desired Length in Dictionary.");
                    }

                }

                //Allowed Guesses Input Validation
                while (maxGuesses <= 0)
                {
                    maxGuesses = InputNum("the desired number of guesses");

                    if (maxGuesses <= 0)
                    {
                        Console.WriteLine("Invalid number of guesses.");
                    }
                    else if (maxGuesses > 26)
                    {
                        Console.WriteLine("Limiting number of guesses to number of Valid guesses in the Alphabet. (26)");
                        maxGuesses = 26;
                    }
                }

                //Display running dictionary count input.
                while (validationBoolean)
                {
                    validationBoolean = false;
                    Console.WriteLine("DISPLAY OPTIONS");
                    Console.WriteLine(" (1)   Display a running total of the remaining valid words in the Dictionary: " + fileName);
                    Console.WriteLine(" (2)   Don't display running total.");
                    inputNum = InputNum("the integer corresponding to the desired option");

                    switch (inputNum)
                    {
                        case 1:
                            printRemainingWords = true;
                            break;
                        case 2:
                            printRemainingWords = false;
                            break;
                        default:
                            Console.WriteLine("Enter a Valid Input.");
                            validationBoolean = true;
                            break;
                    }


                }

                //Guess the Word
                Console.WriteLine("Press any key to Start Game.");
                Console.ReadKey();
                StartGame(wordLength, maxGuesses, printRemainingWords, fileWords);


                validationBoolean = false;
                while (validationBoolean)
                {
                    validationBoolean = true;
                    Console.WriteLine("Would You like to play again?");
                    Console.WriteLine("1) Yes.");
                    Console.WriteLine("2) No.");
                    inputNum = InputNum("the integer corresponding to the desired option");

                    switch (inputNum)
                    {
                        case 1:
                            printRemainingWords = true;
                            break;
                        case 2:
                            printRemainingWords = false;
                            break;
                        default:
                            Console.WriteLine("Enter a Valid Input.");
                            validationBoolean = true;
                            break;
                    }

                }



            }

            
            


        }

        //Method for getting non zero Integer Input, returns zero on Invalid Input
        static int InputNum(string inputType)
        {
            string input;
            int num;

            Console.WriteLine("Enter " + inputType + ".");
            input = Console.ReadLine();

            if (!Int32.TryParse(input, out num))
            {
                //Invalid Input
                Console.WriteLine("ERROR: Invalid Input. ");
                return 0;
            }

            if(num==0)
            {
                //Invalid 0 Input
                Console.WriteLine("ERROR: 0 Invalid. ");
            }

            return num;
        }

        //Method for reading lines from a designated file into a List, but only files of a specified length.
        static List<string> ReadDictionary(string fileName, int wordLength)
        {
            List<string> words = new List<string>();

            string word;


            try
            {
                using (var file = new StreamReader(fileName))
                {
                    // Attempt to read file stream into List of appropriate length strings.

                    while ( (word=file.ReadLine()) != null)
                    {
                        if(word.Length == wordLength)
                        {
                            words.Add(word);
                        }

                    }
                    words.TrimExcess();
                    

                }
            }

            catch (IOException e)
            {
                Console.WriteLine("The file " + fileName + " could not be read:");
                Console.WriteLine(e.Message);
                words = new List<string>();
            }


            return words;
        }
        
        //Method for Starting Process of Playing Guess The Word
        static void StartGame(int wordLen, int allowedGuesses, bool displayWordFamilyInfo, List<string> dictionary)
        {
            //Initialise local Variables
            int remainingGuesses = allowedGuesses;
            bool wordGuessed = false;
            string letterGuessed = "";
            string word = InitWord(wordLen);
            StringBuilder wordStringBuilder;
            List<string> usedLetters = new List<string>();
            List<WordFamily> wordFamilies = new List<WordFamily>();
            WordFamily selectedFamily;

            //Main Game Loop
            while (wordGuessed==false && remainingGuesses > 0)
            {
                //Display Pre Guess data
                PrintPreGuessState(remainingGuesses, usedLetters, word);

                //Print Remaining Words if selected
                if (displayWordFamilyInfo)
                {
                    Console.WriteLine("The Ai still has " + dictionary.Count + " words stored in its' dictionary.");
                }

                //Validate Guess input
                letterGuessed = InputGuess(usedLetters);

                //Partition into word families
                wordFamilies = PartitionDictionary(dictionary, letterGuessed, wordLen);

                //Debug method commented out:
                //PrintAllWordFamilies(wordFamilies);

                //Choose best word family, note commented out line calls unimproved code variant
                //selectedFamily = FindMostCommonWordFamily(wordFamilies, remainingGuesses);
                selectedFamily = FindMostCommonWordFamilyByLetterVariety(wordFamilies, remainingGuesses); //Note Minmax strategy method called.


                //Reduce Dictionary
                dictionary = selectedFamily.wordsInFamily;

                //Update Guess data
                usedLetters.Add(letterGuessed);

                if (selectedFamily.containsGuessCharacter == false)
                {
                    //Incorrect Guess
                    remainingGuesses = remainingGuesses - 1;

                    Console.WriteLine("Incorrect Guess");

                    //Debug example word at start of current word family.
                    //Console.WriteLine("Example Word: " + dictionary[0]);
                }
                else
                {
                    //Correct Guess
                    Console.WriteLine("Correct Guess");

                    wordStringBuilder = new StringBuilder(wordLen);
                    wordStringBuilder.Append(word);
                    foreach (int position in selectedFamily.positionsCharacterPresent)
                    {
                        wordStringBuilder.Remove(position, 1);
                        wordStringBuilder.Insert(position, letterGuessed[0]);

                    }
                    word = wordStringBuilder.ToString();


                    //Check if player won
                    if (dictionary.Count == 1 && word == dictionary[0])
                    {
                        wordGuessed = true;
                    }

                }

            }

            //Winner Output
            if (wordGuessed)
            {
                //Player Won
                Console.WriteLine("You win the word was: " + word);
            }
            else
            {
                //Ai Won
                Console.WriteLine("You lose, the word was: " + dictionary[0]);

                if (displayWordFamilyInfo)
                {
                    Console.WriteLine("The Ai still had " + dictionary.Count + " words in the dictionary.");

                }

            }

            Console.WriteLine("Press any Key to continue");
            Console.ReadKey();
        }

        //Method for displaying game information at the start of the game loop
        static void PrintPreGuessState(int remainingGuesses, List<string> usedLetters, string word)
        {
            Console.WriteLine(remainingGuesses + " guesses remaining.");
            Console.WriteLine("Used Letters: ");

            foreach (string letter in usedLetters)
            {
                Console.Write(letter + " ");
            }

            Console.WriteLine("");
            Console.WriteLine("Word: " + word);
        }

        //Method for Initialising string representing the word the user is trying to guess, with * representing non guessed characters.
        static string InitWord(int wordLen)
        {
            string word = "";

            for (int i = 0; i < wordLen; i++)
            {
                word += "*";
            }

            return word;
        }

        //Method for Validating User's input of Guess Character
        static string InputGuess(List<string> usedLetters)
        {
            string guess = "";
            int guessAsciiValue;

            while(guess == "")
            {
                Console.WriteLine("Please enter the English Alphabet character of your guess.");
                guess = Console.ReadLine();

                guess = guess.ToLower();

                //Check guess a single character.
                if (guess.Length == 1)
                {
                    //Check guess in alphabet
                    guessAsciiValue = guess[0];
                    if (guessAsciiValue >= 97 && guessAsciiValue <= 122)
                    {
                        if (usedLetters.Contains(guess))
                        {
                            //Guess already made.
                            Console.WriteLine(guess + " has already been guessed.");
                            guess = "";
                        }
                    }
                    else
                    {
                        Console.WriteLine("Please enter a single character in the standard english alphabet.");
                        guess = "";
                    }
                }
                else
                {
                    Console.WriteLine("Please enter a single character.");
                    guess = "";
                }

            }
            return guess;
        }

        //Method for Partitioning Dictionary into word families, stored as objects in a List structure.
        static List<WordFamily> PartitionDictionary(List<string> dictionary, string guess, int wordLength)
        {
            char letter = guess[0];
            bool inFamily=false, inWordFamilies=false;


            List<WordFamily> wordFamilies = new List<WordFamily>();
            List<int> inputPositions = new List<int>();
            wordFamilies.Add(new WordFamily());


            foreach (string word in dictionary)
            {
                //Add word to appropriate Family
                inFamily = false;
                inputPositions = new List<int>();

                for (int i = 0; i < wordLength; i++)
                {
                    if (word[i] == letter)
                    {
                        inFamily = true;
                        inputPositions.Add(i);
                    }
                }

                if (inFamily)
                {
                    inWordFamilies = false;

                    //Check for existing word family, associated with guess at found input positions
                    foreach (WordFamily family in wordFamilies)
                    {
                        if (family.containsGuessCharacter)
                        {
                            if (family.positionsCharacterPresent.SequenceEqual(inputPositions))
                            {
                                family.AddWord(word);
                                inWordFamilies = true;
                            }
                        }
                    }

                    if(inWordFamilies==false)
                    {
                        //Add word family object, associated with guess at found input positions
                        wordFamilies.Add(new WordFamily(letter, inputPositions, word));
                    }

                }
                else
                {
                    //Add word to family not Associated with guess
                    wordFamilies[0].AddWord(word);
                }
            }


            return wordFamilies;
        }

        //Method for Selecting Word family by identifying family with the largest number of words.
        static WordFamily FindMostCommonWordFamily(List<WordFamily> wordFamilies, int remainingGuesses)
        {
            WordFamily outputFamily = new WordFamily(); //Note Empty Word Family instantiated here should never be returned.
            int currentWordCount = 0;

            if (remainingGuesses == 1 && wordFamilies[0].wordsInFamily.Count > 0)
            {
                //1 Remaining Guess and Ai has unguessed word available to pick.
                outputFamily = wordFamilies[0];
            }
            else
            {
                foreach(WordFamily family in wordFamilies)
                {
                    if (currentWordCount < family.wordCount)
                    {
                        //If family currently being evaluated has a higher wordcount, replace selection
                        outputFamily = family;
                        currentWordCount = family.wordCount;
                    }
                }
            }

            return outputFamily;
        }

        //Optimisation Method for selecting word family with the largest variety of letters.
        static WordFamily FindMostCommonWordFamilyByLetterVariety(List<WordFamily> wordFamilies, int remainingGuesses)
        {
            WordFamily outputFamily = new WordFamily(); //Note Empty Word Family instantiated here should never be returned.
            int currentLetterVariety = 0;

            if (remainingGuesses == 1 && wordFamilies[0].wordsInFamily.Count > 0)
            {
                //1 Remaining Guess and Ai has unguessed word available to pick.
                outputFamily = wordFamilies[0];
            }
            else
            {
                foreach (WordFamily family in wordFamilies)
                {
                    if (currentLetterVariety < family.letterVariety)
                    {
                        //If family currently being evaluated has a higher word variety, replace selection
                        outputFamily = family;
                        currentLetterVariety = family.letterVariety;
                    }
                }
            }
            return outputFamily;


        }

        //Debug method for outputting all data associated with each word family process to better debug the decision making process on small data sets, such as the files test.txt and test2.txt
        static void PrintAllWordFamilies(List<WordFamily> wordFamilies)
        {
            int fam = 0;
            foreach (WordFamily family in wordFamilies)
            {
                Console.WriteLine("Family " + fam + ":");
                Console.WriteLine("Num Words: " + family.wordCount + " Variety Letters: " + family.letterVariety);
                family.PrintWords();
                fam++;
            }
        }

    }

    //Class for storing all the data about a given word family to be stored in a List data structure
    public class WordFamily
    {
        public bool containsGuessCharacter;
        public char guessCharacter;
        public int wordCount, letterVariety;
        public List<int> positionsCharacterPresent;
        public List<string> wordsInFamily, lettersInFamily;

        //Constructor for Family[0], the potentially empty family containing zero matches with guessed letters
        public WordFamily()
        {
            wordCount = 0;
            letterVariety = 0;
            containsGuessCharacter = false;
            wordsInFamily = new List<string>();
            lettersInFamily = new List<string>();
        }

        //Constructor for Unique word Family[N], containing at least one word and containing at unique presence of the guessed letter
        public WordFamily(char inputGuess, List<int> inputPositions, string word)
        {
            wordCount = 0;
            letterVariety = 0;
            containsGuessCharacter = true;
            guessCharacter = inputGuess;
            positionsCharacterPresent = inputPositions;
            wordsInFamily = new List<string>();
            lettersInFamily = new List<string>();
            AddWord(word);
        }

        //Method for adding word to Storage and incrementing wordcount
        public void AddWord(string word)
        {
            wordsInFamily.Add(word);
            wordCount = wordCount + 1;
            string letter;

            foreach(char character in word)
            {
                letter = character.ToString();
                AddLetter(letter);
            }
        }

        //Method for Adding Letter to Letter Variety Log and Storage
        private void AddLetter(string letter)
        {
            bool letterAlreadyPresent = false;

            foreach (string letterPresent in lettersInFamily)
            {
                if (letter == letterPresent)
                {
                    letterAlreadyPresent = true;
                }
            }

            if (letterAlreadyPresent == false)
            {
                lettersInFamily.Add(letter);
                letterVariety = letterVariety + 1;
            }
        }

        //Debug method for outputting Words In word family
        public void PrintWords()
        {
            foreach (string word in wordsInFamily)
            {
                Console.WriteLine(word);
            }
        }
    }
}
