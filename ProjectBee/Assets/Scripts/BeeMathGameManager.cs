using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BeeMathGameManager : MonoBehaviour
{
    public enum GameMode
    {
        Addition,
        Subtraction,
        Multiplication
    }

    public enum GameState
    {
        Playing,
        Resolving,
        Win,
        Lose
    }

    [Header("MODE")]
    [SerializeField] private GameMode currentMode = GameMode.Addition;

    [Header("FLOWERS")]
    [SerializeField] private Flower flowerPrefab;
    [SerializeField] private Transform flowerContainer;
    [SerializeField] private Transform[] flowerSpawnPoints;
    [SerializeField] private int numberOfFlowers = 6;

    [Header("BEES")]
    [SerializeField] private Bee beePrefab;
    [SerializeField] private Transform[] beeSpawnPoints;

    [Header("UI")]
    [SerializeField] private Slider honeySlider;
    [SerializeField] private TMP_Text modeText;
    [SerializeField] private TMP_Text problemText;

    [Header("HONEY")]
    [SerializeField] private float maxHoney = 100f;
    [SerializeField] private float startingHoney = 50f;
    [SerializeField] private float honeyDrainPerSecond = 2f;
    [SerializeField] private float correctAnswerHoney = 15f;

    [Header("PROBLEM SETTINGS")]
    [SerializeField] private int minNumber = 1;
    [SerializeField] private int maxNumber = 10;

    [Header("TIMING")]
    [SerializeField] private float nextProblemDelay = 1.5f;

    private float currentHoney;

    private int targetAnswer;

    // Addition
    private Flower firstSelectedFlower;
    private Flower secondSelectedFlower;

    // Multiplication
    private int multiplicationBeeCount;
    private int multiplicationAmountPerBee;

    private GameState currentState;

    private readonly List<Flower> activeFlowers = new();
    private readonly List<Bee> activeBees = new();

    private void Start()
    {
        currentHoney = startingHoney;

        honeySlider.minValue = 0f;
        honeySlider.maxValue = maxHoney;

        UpdateHoneyUI();

        StartNewProblem();
    }

    private void Update()
    {
        if (currentState != GameState.Playing)
            return;

        currentHoney -= honeyDrainPerSecond * Time.deltaTime;
        currentHoney = Mathf.Clamp(currentHoney, 0f, maxHoney);

        UpdateHoneyUI();

        if (currentHoney <= 0f)
        {
            LoseGame();
        }
    }

    // =========================================================
    // START PROBLEM
    // =========================================================

    public void StartNewProblem()
    {
        currentState = GameState.Playing;

        ClearCurrentProblem();

        modeText.text = currentMode.ToString();

        switch (currentMode)
        {
            case GameMode.Addition:
                CreateAdditionProblem();
                break;

            case GameMode.Subtraction:
                CreateSubtractionProblem();
                break;

            case GameMode.Multiplication:
                CreateMultiplicationProblem();
                break;
        }
    }

    // =========================================================
    // ADDITION
    // =========================================================

    private void CreateAdditionProblem()
    {
        /*
         Example:

         Bee wants 8

         Valid flowers:
         3 + 5 = 8

         Player must select TWO flowers.
        */

        int firstAnswer = Random.Range(minNumber, maxNumber);

        int minimumSecondValue = Mathf.Max(1, minNumber);
        int secondAnswer = Random.Range(
            minimumSecondValue,
            maxNumber + 1
        );

        targetAnswer = firstAnswer + secondAnswer;

        SpawnSingleBee(targetAnswer);

        problemText.text =
            $"Find two flowers that add up to {targetAnswer}";

        List<int> values = new List<int>();

        // Guaranteed correct pair.
        values.Add(firstAnswer);
        values.Add(secondAnswer);

        // Fill remaining flowers with distractors.
        while (values.Count < numberOfFlowers)
        {
            int randomValue = Random.Range(minNumber, maxNumber + 1);

            values.Add(randomValue);
        }

        Shuffle(values);

        SpawnFlowers(values);
    }

    // =========================================================
    // SUBTRACTION
    // =========================================================

    private void CreateSubtractionProblem()
    {
        /*
         Example:

         Bee wants 5.

         Flower starts at 8.

         Player removes:
         8 -> 7 -> 6 -> 5

         Then selects the flower.
        */

        targetAnswer = Random.Range(minNumber, maxNumber + 1);

        SpawnSingleBee(targetAnswer);

        problemText.text =
            $"Make a flower equal {targetAnswer}";

        List<int> values = new List<int>();

        // Create at least one flower that can be reduced
        // down to the correct answer.
        int removableAmount = Random.Range(1, 4);

        int correctStartingValue =
            targetAnswer + removableAmount;

        values.Add(correctStartingValue);

        while (values.Count < numberOfFlowers)
        {
            int randomValue = Random.Range(
                minNumber,
                maxNumber + 4
            );

            values.Add(randomValue);
        }

        Shuffle(values);

        SpawnFlowers(values);
    }

    // =========================================================
    // MULTIPLICATION
    // =========================================================

    private void CreateMultiplicationProblem()
    {
        /*
         Example:

         3 bees
         Each wants 4

         Target:
         3 x 4 = 12

         Player selects flower 12.
        */

        multiplicationBeeCount = Random.Range(2, 5);

        multiplicationAmountPerBee =
            Random.Range(2, 6);

        targetAnswer =
            multiplicationBeeCount *
            multiplicationAmountPerBee;

        problemText.text =
            $"{multiplicationBeeCount} bees need " +
            $"{multiplicationAmountPerBee} pollen each";

        SpawnBeeGroup(
            multiplicationBeeCount,
            multiplicationAmountPerBee
        );

        List<int> values = new List<int>();

        // Guaranteed answer.
        values.Add(targetAnswer);

        while (values.Count < numberOfFlowers)
        {
            int offset = Random.Range(-5, 6);

            int randomValue =
                Mathf.Max(1, targetAnswer + offset);

            if (randomValue == targetAnswer)
                continue;

            values.Add(randomValue);
        }

        Shuffle(values);

        SpawnFlowers(values);
    }

    // =========================================================
    // FLOWER INTERACTION
    // =========================================================

    public void SelectFlower(Flower flower)
    {
        if (currentState != GameState.Playing)
            return;

        switch (currentMode)
        {
            case GameMode.Addition:
                HandleAdditionSelection(flower);
                break;

            case GameMode.Subtraction:
                HandleSingleFlowerSelection(flower);
                break;

            case GameMode.Multiplication:
                HandleSingleFlowerSelection(flower);
                break;
        }
    }

    private void HandleAdditionSelection(Flower flower)
    {
        if (firstSelectedFlower == null)
        {
            firstSelectedFlower = flower;

            flower.SetSelected(true);

            return;
        }

        // Clicking same flower again deselects it.
        if (firstSelectedFlower == flower)
        {
            firstSelectedFlower.SetSelected(false);

            firstSelectedFlower = null;

            return;
        }

        secondSelectedFlower = flower;

        flower.SetSelected(true);

        int total =
            firstSelectedFlower.Value +
            secondSelectedFlower.Value;

        if (total == targetAnswer)
        {
            CorrectAnswer();
        }
        else if (total > targetAnswer)
        {
            IncorrectAnswer();
        }
        else
        {
            // Wrong, but not too high.
            // Reset selection and let player try again.

            StartCoroutine(
                ResetAdditionSelection()
            );
        }
    }

    private IEnumerator ResetAdditionSelection()
    {
        currentState = GameState.Resolving;

        yield return new WaitForSeconds(0.4f);

        if (firstSelectedFlower != null)
            firstSelectedFlower.SetSelected(false);

        if (secondSelectedFlower != null)
            secondSelectedFlower.SetSelected(false);

        firstSelectedFlower = null;
        secondSelectedFlower = null;

        currentState = GameState.Playing;
    }

    private void HandleSingleFlowerSelection(Flower flower)
    {
        if (flower.Value == targetAnswer)
        {
            CorrectAnswer();
        }
        else if (flower.Value > targetAnswer)
        {
            IncorrectAnswer();
        }
        else
        {
            // Too low.
            IncorrectAnswer();
        }
    }

    // =========================================================
    // PETAL REMOVAL
    // =========================================================

    public void RemovePetal(Flower flower)
    {
        if (currentState != GameState.Playing)
            return;

        // Primarily intended for subtraction mode.
        if (currentMode != GameMode.Subtraction)
            return;

        flower.RemovePetal();

        // User specifically said:
        // if number gets too high, bee turns red and leaves.
        //
        // In subtraction, removing petals makes the number smaller,
        // so going BELOW the target means they can no longer recover.

        if (flower.Value < targetAnswer)
        {
            IncorrectAnswer();
        }
    }

    // =========================================================
    // CORRECT / WRONG
    // =========================================================

    private void CorrectAnswer()
    {
        if (currentState != GameState.Playing)
            return;

        currentState = GameState.Resolving;

        currentHoney += correctAnswerHoney;

        currentHoney = Mathf.Clamp(
            currentHoney,
            0f,
            maxHoney
        );

        UpdateHoneyUI();

        foreach (Bee bee in activeBees)
        {
            if (bee != null)
                bee.Success();
        }

        if (currentHoney >= maxHoney)
        {
            WinGame();
            return;
        }

        StartCoroutine(
            WaitForNextProblem()
        );
    }

    private void IncorrectAnswer()
    {
        if (currentState != GameState.Playing)
            return;

        currentState = GameState.Resolving;

        foreach (Bee bee in activeBees)
        {
            if (bee != null)
                bee.Fail();
        }

        StartCoroutine(
            WaitForNextProblem()
        );
    }

    private IEnumerator WaitForNextProblem()
    {
        yield return new WaitForSeconds(
            nextProblemDelay
        );

        StartNewProblem();
    }

    // =========================================================
    // BEES
    // =========================================================

    private void SpawnSingleBee(int requestedAmount)
    {
        Bee bee = Instantiate(
            beePrefab,
            beeSpawnPoints[0].position,
            Quaternion.identity
        );

        bee.Initialize(requestedAmount);

        activeBees.Add(bee);
    }

    private void SpawnBeeGroup(
        int beeCount,
        int amountPerBee
    )
    {
        for (int i = 0; i < beeCount; i++)
        {
            Transform spawnPoint =
                beeSpawnPoints[
                    i % beeSpawnPoints.Length
                ];

            Bee bee = Instantiate(
                beePrefab,
                spawnPoint.position,
                Quaternion.identity
            );

            bee.Initialize(amountPerBee);

            activeBees.Add(bee);
        }
    }

    // =========================================================
    // FLOWERS
    // =========================================================

    private void SpawnFlowers(List<int> values)
    {
        int count = Mathf.Min(
            values.Count,
            flowerSpawnPoints.Length
        );

        for (int i = 0; i < count; i++)
        {
            Flower flower = Instantiate(
                flowerPrefab,
                flowerSpawnPoints[i].position,
                Quaternion.identity,
                flowerContainer
            );

            flower.Initialize(
                values[i],
                this
            );

            activeFlowers.Add(flower);
        }
    }

    // =========================================================
    // CLEANUP
    // =========================================================

    private void ClearCurrentProblem()
    {
        StopAllCoroutines();

        foreach (Flower flower in activeFlowers)
        {
            if (flower != null)
                Destroy(flower.gameObject);
        }

        foreach (Bee bee in activeBees)
        {
            if (bee != null)
                Destroy(bee.gameObject);
        }

        activeFlowers.Clear();
        activeBees.Clear();

        firstSelectedFlower = null;
        secondSelectedFlower = null;
    }

    // =========================================================
    // HONEY
    // =========================================================

    private void UpdateHoneyUI()
    {
        honeySlider.value = currentHoney;
    }

    // =========================================================
    // WIN / LOSE
    // =========================================================

    private void WinGame()
    {
        currentState = GameState.Win;

        problemText.text = "YOU WIN!";
    }

    private void LoseGame()
    {
        currentState = GameState.Lose;

        problemText.text = "OUT OF HONEY!";
    }

    // =========================================================
    // MODE CHANGE
    // =========================================================

    public void SetAdditionMode()
    {
        currentMode = GameMode.Addition;
        StartNewProblem();
    }

    public void SetSubtractionMode()
    {
        currentMode = GameMode.Subtraction;
        StartNewProblem();
    }

    public void SetMultiplicationMode()
    {
        currentMode = GameMode.Multiplication;
        StartNewProblem();
    }

    // =========================================================
    // UTILITY
    // =========================================================

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex =
                Random.Range(i, list.Count);

            (list[i], list[randomIndex]) =
                (list[randomIndex], list[i]);
        }
    }
}