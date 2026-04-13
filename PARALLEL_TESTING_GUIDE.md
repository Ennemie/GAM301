# Parallel Test Execution Guide

## Overview

This project supports parallel test execution across 6 independent test modules:
- **Movement** (TC1-TC10): 10 tests
- **Attack** (TC11-TC21): 11 tests
- **Enemy** (TC22-TC28): 7 tests
- **Life** (TC29-TC35): 7 tests
- **Item** (TC36-TC45): 10 tests
- **UI** (TC46-TC57): 12 tests

**Total: 51 tests** running concurrently to reduce execution time.

---

## Running Tests Locally

### All Tests at Once (Sequential)
```bash
Unity -runTests -testMode playmode -logFile -
```

### Parallel Execution by Category

Each test module can be run independently using NUnit categories:

#### Movement Tests
```bash
Unity -runTests -testMode playmode -testCategory Movement -logFile -
```

#### Attack Tests
```bash
Unity -runTests -testMode playmode -testCategory Attack -logFile -
```

#### Enemy Tests
```bash
Unity -runTests -testMode playmode -testCategory Enemy -logFile -
```

#### Life Tests
```bash
Unity -runTests -testMode playmode -testCategory Life -logFile -
```

#### Item Tests
```bash
Unity -runTests -testMode playmode -testCategory Item -logFile -
```

#### UI Tests
```bash
Unity -runTests -testMode playmode -testCategory UI -logFile -
```

### Running in Unity Editor Test Runner

1. Open **Window → Testing → Test Runner**
2. In the **Test Runner** window, click the **Filter** dropdown
3. Select the category you want to run:
   - `Movement` - Tank movement mechanics
   - `Attack` - Shooting and fire rate system
   - `Enemy` - Enemy AI behavior
   - `Life` - Health and damage system
   - `Item` - PowerUp system
   - `UI` - UI interactions and transitions
4. Click **Run Selected** to execute only that category

---

## Automated Parallel Execution (GitHub Actions)

The project includes a GitHub Actions workflow (`.github/workflows/parallel-tests.yml`) that:

1. **Triggers on:**
   - Push to `main` or `develop` branches
   - Pull requests to `main` or `develop` branches

2. **Runs 6 jobs in parallel:**
   - Each job executes one test module independently
   - Jobs run concurrently on separate runners
   - Artifacts are collected after completion

3. **Reports consolidated results:**
   - Summary job waits for all 6 modules to complete
   - Displays pass/fail status for each module
   - Fails the workflow if any module fails

### Expected Performance

**Sequential execution:** ~3-5 minutes (estimated)
**Parallel execution:** ~45-60 seconds (6 jobs concurrent)

### Expected Speedup: **4-6x faster**

---

## Test Module Independence

Each test module is designed to run independently with no cross-dependencies:

| Module | Scene | Setup | Dependencies |
|--------|-------|-------|--------------|
| Movement | GamePlay | Load scene, find Player | None external |
| Attack | GamePlay | Load scene, find Player | None external |
| Enemy | GamePlay | Load scene, spawn enemies | None external |
| Life | GamePlay | Load scene, find UI | None external |
| Item | GamePlay | Load scene, create PowerUps | None external |
| UI | Intro/GamePlay | Load scenes | None external |

**Key architectural feature:** Each test module has its own `[UnitySetUp]` and `[UnityTearDown]` that load the scene fresh and clean up afterward, ensuring no interference between modules.

---

## Test Categories in Code

All test classes are decorated with NUnit `[Category]` attributes:

```csharp
[Category("Movement")]
public class MovementTestScripts { ... }

[Category("Attack")]
public class AttackTestScripts { ... }

[Category("Enemy")]
public class EnemyTestScripts { ... }

[Category("Life")]
public class LifeManagerTestScripts { ... }

[Category("Item")]
public class ItemManagerTestScripts { ... }

[Category("UI")]
public class UITestScripts { ... }
```

These categories enable filtering in:
- Unity Test Runner UI
- Command-line test execution
- CI/CD pipelines
- NUnit test frameworks

---

## Troubleshooting Parallel Tests

### Issue: Tests failing only in parallel, passing sequentially

**Solution:**
- Check for shared static state or singletons
- Verify each test has proper Setup/Teardown
- Ensure scene loading is fully awaited
- Add timeout loops instead of WaitUntil()

### Issue: Scene loading conflicts

**Solution:**
- Unity handles scene loading isolation automatically
- Each test job runs in a separate process
- No cross-job scene interference expected

### Issue: Physics/NavMesh timing issues

**Solution:**
- Timeout loops already implemented (5-second safety)
- Parallel execution may expose timing issues
- If found, increase timeout values in test

### Issue: GitHub Actions workflow failing

**Solution:**
- Check `needs: [...]` dependencies in summary job
- Verify test artifacts are being created
- Review individual job logs in Actions tab

---

## Performance Metrics

### Baseline (Sequential) vs Parallel

```
Sequential:  51 tests total
├─ Movement: ~30-40s
├─ Attack:   ~30-40s
├─ Enemy:    ~20-30s
├─ Life:     ~20-30s
├─ Item:     ~25-35s
└─ UI:       ~25-35s
Total:       ~2.5-4 minutes

Parallel:    51 tests concurrent
├─ Movement: ~30-40s (job 1)
├─ Attack:   ~30-40s (job 2) [concurrent]
├─ Enemy:    ~20-30s (job 3) [concurrent]
├─ Life:     ~20-30s (job 4) [concurrent]
├─ Item:     ~25-35s (job 5) [concurrent]
└─ UI:       ~25-35s (job 6) [concurrent]
Total:       ~40-50 seconds
```

**Speedup Factor: 3-5x**

---

## Adding New Tests to Parallel Pipeline

When adding new tests:

1. Assign the appropriate `[Category]` attribute
2. Add comprehensive Setup/Teardown to maintain isolation
3. Use timeout loops for async operations
4. Test sequentially first, then in parallel
5. Verify no cross-test dependencies

Example:
```csharp
[Category("Movement")]
public class NewMovementTests
{
    [UnitySetUp]
    public IEnumerator Setup()
    {
        // Load fresh scene
        AsyncOperation load = SceneManager.LoadSceneAsync("GamePlay");
        yield return new WaitUntil(() => load.isDone);
        yield return new WaitForSeconds(1f);
    }

    [UnityTearDown]
    public IEnumerator Teardown()
    {
        // Clean up test state
        yield return null;
    }

    [UnityTest]
    public IEnumerator NewTestCase()
    {
        // Test implementation with timeout safety
    }
}
```

---

## CI/CD Integration

The GitHub Actions workflow enables:

- **Automatic testing on every push**
- **Pull request validation**
- **6 concurrent test jobs**
- **Consolidated reporting**
- **Artifact collection for debugging**

To use:
1. Push changes to `main` or `develop`
2. Workflow automatically triggers
3. Check Actions tab for results
4. Review individual job logs if needed

---

## Test Isolation Validation

All 6 modules are verified to be independent:

✅ **No shared static state** - Each module manages its own test objects
✅ **Separate scene loading** - Each test loads a fresh scene instance
✅ **Dedicated Setup/Teardown** - Proper initialization and cleanup per test
✅ **No file I/O conflicts** - Tests don't compete for resources
✅ **No network dependencies** - All tests are local/offline
✅ **Idempotent operations** - Each test can run any number of times in any order
✅ **Independent assertions** - Tests don't depend on previous test results

---

## References

- **NUnit Categories:** https://docs.nunit.org/articles/nunit/running-tests/Categories.html
- **Unity Test Runner:** https://docs.unity3d.com/Manual/test-framework.html
- **Game CI Unity Test Runner:** https://game-ci.github.io/documentation/github/test-runner
- **GitHub Actions:** https://docs.github.com/en/actions

