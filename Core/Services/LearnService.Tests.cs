using MemoryLingo.Core.Models;
using MemoryLingo.Infrastructure.Settings;
using MemoryLingo.Infrastructure.VocabularyExcel;
using MemoryLingo.Infrastructure.VocabularyProgress;
using MemoryLingo.Infrastructure.VocabularyReference;
using NSubstitute;
using Xunit;

namespace MemoryLingo.Core.Services;

public class LearnServiceTests
{
	#region LoadSession
	[Fact]
	public void LoadSession0_WithContinueTrue_CopiesFiveEntriesFromVocabularyProgress()
	{
		// Arrange
		var (settingsStore, vocabularyProgressStore, vocabularyReferenceStore, vocabularyExcelReader) = CreateMocksForLoadSession();

		// Create test progress with 5 unlearned entries and 1 learned entry for session 0
		var vocabularyProgress = new VocabularyProgressDto
		{
			Entries = new Dictionary<string, VocabularyProgressEntry>
			{
				{ "word1", new VocabularyProgressEntry
					{
						Sessions = [new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false }]
					}
				},
				{ "word2", new VocabularyProgressEntry
					{
						Sessions = [new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false }]
					}
				},
				{ "word3", new VocabularyProgressEntry
					{
						Sessions = [new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false }]
					}
				},
				{ "word4", new VocabularyProgressEntry
					{
						Sessions = [new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false }]
					}
				},
				{ "word5", new VocabularyProgressEntry
					{
						Sessions = [new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = true }]
					}
				},
				{ "word6", new VocabularyProgressEntry
					{
						Sessions = [new VocabularyProgressEntrySession { IsLearned = true, IsSkipped = false }]
					}
				}
			}
		};
		vocabularyProgressStore.Load("test.xlsx").Returns(vocabularyProgress);

		var learnService = new LearnService(settingsStore, vocabularyProgressStore, vocabularyReferenceStore, vocabularyExcelReader);

		// Act
		var vocabularyResult = learnService.StartVocabularySession("test.xlsx", 0, true);
		var sessionLoaded = learnService.LoadSession(0, true);

		// Assert
		Assert.True(sessionLoaded);
		Assert.NotNull(vocabularyResult);

		var currentSession = learnService.GetCurrentSession();
		Assert.NotNull(currentSession);
		var entriesInQueue = currentSession.Entries.Keys.ToList();
		Assert.Equal(4, entriesInQueue.Count);
		Assert.Contains("word1", entriesInQueue);
		Assert.Contains("word2", entriesInQueue);
		Assert.Contains("word3", entriesInQueue);
		Assert.Contains("word4", entriesInQueue);
		Assert.DoesNotContain("word5", entriesInQueue); // This should be excluded as it's skipped
		Assert.DoesNotContain("word6", entriesInQueue); // This should be excluded as it's already learned
	}

	[Fact]
	public void LoadSession1_WithContinueTrue_CopiesFiveEntriesFromVocabularyProgress()
	{
		// Arrange
		var (settingsStore, vocabularyProgressStore, vocabularyReferenceStore, vocabularyExcelReader) = CreateMocksForLoadSession();

		// Create test progress with 3 sessions for each entry
		var vocabularyProgress = new VocabularyProgressDto
		{
			Entries = new Dictionary<string, VocabularyProgressEntry>
			{
				{ "word1", new VocabularyProgressEntry
					{
						Sessions = [
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false },
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false },
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false }
						]
					}
				},
				{ "word2", new VocabularyProgressEntry
					{
						Sessions = [
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false },
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false },
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false }
						]
					}
				},
				{ "word3", new VocabularyProgressEntry
					{
						Sessions = [
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false },
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false },
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false }
						]
					}
				},
				{ "word4", new VocabularyProgressEntry
					{
						Sessions = [
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false },
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false },
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false }
						]
					}
				},
				{ "word5", new VocabularyProgressEntry
					{
						Sessions = [
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false },
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = true },
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false }
						]
					}
				},
				{ "word6", new VocabularyProgressEntry
					{
						Sessions = [
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false },
							new VocabularyProgressEntrySession { IsLearned = true, IsSkipped = false },
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false }
						]
					}
				}
			}
		};
		vocabularyProgressStore.Load("test.xlsx").Returns(vocabularyProgress);

		var learnService = new LearnService(settingsStore, vocabularyProgressStore, vocabularyReferenceStore, vocabularyExcelReader);

		// Act
		var vocabularyResult = learnService.StartVocabularySession("test.xlsx", 2, true);
		var sessionLoaded = learnService.LoadSession(1, true);

		// Assert
		Assert.True(sessionLoaded);
		Assert.NotNull(vocabularyResult);

		var currentSession = learnService.GetCurrentSession();
		Assert.NotNull(currentSession);
		var entriesInQueue = currentSession.Entries.Keys.ToList();
		Assert.Equal(4, entriesInQueue.Count);
		Assert.Contains("word1", entriesInQueue);
		Assert.Contains("word2", entriesInQueue);
		Assert.Contains("word3", entriesInQueue);
		Assert.Contains("word4", entriesInQueue);
		Assert.DoesNotContain("word5", entriesInQueue); // This should be excluded as it's skipped in session 2
		Assert.DoesNotContain("word6", entriesInQueue); // This should be excluded as it's already learned in session 2
	}

	[Fact]
	public void LoadSession1_WithContinueFalse_Filters30PercentFromPreviousSession()
	{
		// Arrange
		var (settingsStore, vocabularyProgressStore, vocabularyReferenceStore, vocabularyExcelReader) = CreateMocksForLoadSession(entryCount: 7);

		// Create test progress with different TotalAttempts in session 0 to test ordering
		var vocabularyProgress = new VocabularyProgressDto
		{
			Entries = new Dictionary<string, VocabularyProgressEntry>
			{
				{ "word1", new VocabularyProgressEntry
					{
						Sessions = [
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false, TotalAttempts = 5 }, // Most difficult
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = true }
						]
					}
				},
				{ "word2", new VocabularyProgressEntry
					{
						Sessions = [
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false, TotalAttempts = 4 }, // Second most difficult
							new VocabularyProgressEntrySession { IsLearned = true, IsSkipped = false }
						]
					}
				},
				{ "word3", new VocabularyProgressEntry
					{
						Sessions = [
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false, TotalAttempts = 3 },
							new VocabularyProgressEntrySession { IsLearned = true, IsSkipped = true }
						]
					}
				},
				{ "word4", new VocabularyProgressEntry
					{
						Sessions = [
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false, TotalAttempts = 2 },
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false }
						]
					}
				},
				{ "word5", new VocabularyProgressEntry
					{
						Sessions = [
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false, TotalAttempts = 1 },
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false }
						]
					}
				},
				{ "word6", new VocabularyProgressEntry
					{
						Sessions = [
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false, TotalAttempts = 1 },
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = true }
						]
					}
				},
				{ "word7", new VocabularyProgressEntry
					{
						Sessions = [
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false, TotalAttempts = 1 },
							new VocabularyProgressEntrySession { IsLearned = true, IsSkipped = false }
						]
					}
				}
			}
		};
		vocabularyProgressStore.Load("test.xlsx").Returns(vocabularyProgress);

		var learnService = new LearnService(settingsStore, vocabularyProgressStore, vocabularyReferenceStore, vocabularyExcelReader);

		// Act
		var vocabularyResult = learnService.StartVocabularySession("test.xlsx", 1, false);
		var sessionLoaded = learnService.LoadSession(1, false);

		// Assert
		Assert.True(sessionLoaded);
		Assert.NotNull(vocabularyResult);

		var currentSession = learnService.GetCurrentSession();
		Assert.NotNull(currentSession);
		var entriesInQueue = currentSession.Entries.Keys.ToList();

		// With 30% of 7 entries = 2 entries (the most difficult ones from session 0)
		Assert.Equal(2, entriesInQueue.Count);
		Assert.Contains("word1", entriesInQueue); // Highest TotalAttempts (5)
		Assert.Contains("word2", entriesInQueue); // Second highest TotalAttempts (4)
		Assert.DoesNotContain("word3", entriesInQueue); // Should be skipped
		Assert.DoesNotContain("word4", entriesInQueue); // Should be skipped
		Assert.DoesNotContain("word5", entriesInQueue); // Should be skipped
		Assert.DoesNotContain("word6", entriesInQueue); // Should be skipped
		Assert.DoesNotContain("word7", entriesInQueue); // Should be skipped
	}

	[Fact]
	public void LoadSession2_WithContinueFalse_Filters50PercentIgnoringSkippedEntries()
	{
		// Arrange
		var (settingsStore, vocabularyProgressStore, vocabularyReferenceStore, vocabularyExcelReader) = CreateMocksForLoadSession();

		// Create test progress where the two highest-rated entries in session 1 are skipped
		var vocabularyProgress = new VocabularyProgressDto
		{
			Entries = new Dictionary<string, VocabularyProgressEntry>
			{
				{ "word1", new VocabularyProgressEntry
					{
						Sessions = [
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false },
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = true, TotalAttempts = 10 }, // Highest but skipped
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false }
						]
					}
				},
				{ "word2", new VocabularyProgressEntry
					{
						Sessions = [
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false },
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = true, TotalAttempts = 9 }, // Second highest but skipped
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false }
						]
					}
				},
				{ "word3", new VocabularyProgressEntry
					{
						Sessions = [
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false },
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false, TotalAttempts = 8 }, // Highest non-skipped
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false }
						]
					}
				},
				{ "word4", new VocabularyProgressEntry
					{
						Sessions = [
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false },
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false, TotalAttempts = 7 }, // Second highest non-skipped
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false }
						]
					}
				},
				{ "word5", new VocabularyProgressEntry
					{
						Sessions = [
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false },
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false, TotalAttempts = 6 },
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false }
						]
					}
				},
				{ "word6", new VocabularyProgressEntry
					{
						Sessions = [
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false },
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false, TotalAttempts = 5 },
							new VocabularyProgressEntrySession { IsLearned = false, IsSkipped = false }
						]
					}
				}
			}
		};
		vocabularyProgressStore.Load("test.xlsx").Returns(vocabularyProgress);

		var learnService = new LearnService(settingsStore, vocabularyProgressStore, vocabularyReferenceStore, vocabularyExcelReader);

		// Act
		var vocabularyResult = learnService.StartVocabularySession("test.xlsx", 2, false);
		var sessionLoaded = learnService.LoadSession(2, false);

		// Assert
		Assert.True(sessionLoaded);
		Assert.NotNull(vocabularyResult);

		var currentSession = learnService.GetCurrentSession();
		Assert.NotNull(currentSession);
		var entriesInQueue = currentSession.Entries.Keys.ToList();

		// Only non-skipped entries from session 1 are considered: word3, word4, word5, word6 (4 entries)
		// 50% of 4 entries = 2 entries (the most difficult non-skipped ones from session 1)
		Assert.Equal(2, entriesInQueue.Count);
		Assert.Contains("word3", entriesInQueue); // Highest TotalAttempts among non-skipped (8)
		Assert.Contains("word4", entriesInQueue); // Second highest TotalAttempts among non-skipped (7)
		Assert.DoesNotContain("word1", entriesInQueue); // Should be skipped (was skipped in session 1)
		Assert.DoesNotContain("word2", entriesInQueue); // Should be skipped (was skipped in session 1)
		Assert.DoesNotContain("word5", entriesInQueue); // Should be skipped (excluded by percentage)
		Assert.DoesNotContain("word6", entriesInQueue); // Should be skipped (excluded by percentage)
	}

	static (ISettingsStore, IVocabularyProgressStore, IVocabularyReferenceService, IVocabularyExcelReader) CreateMocksForLoadSession(int entryCount = 6)
	{
		int session2Percent = 30;
		int session3Percent = 50;
		int exerciseSize = 5;
		int correctAnswersToLearn = 2;

		var settingsStore = Substitute.For<ISettingsStore>();
		var vocabularyProgressStore = Substitute.For<IVocabularyProgressStore>();
		var vocabularyReferenceStore = Substitute.For<IVocabularyReferenceService>();
		var vocabularyExcelReader = Substitute.For<IVocabularyExcelReader>();

		var settings = new SettingsDto
		{
			Learn = new LearnSettings
			{
				ExerciseSize = exerciseSize,
				CorrectAnswersToLearn = correctAnswersToLearn,
				DifficultEntriesSession2Percent = session2Percent,
				DifficultEntriesSession3Percent = session3Percent
			}
		};
		settingsStore.Load().Returns(settings);

		var entries = new List<Entry>();
		for (int i = 1; i <= entryCount; i++)
		{
			entries.Add(new Entry { RuText = $"word{i}", EnText = $"translation{i}" });
		}

		var vocabulary = new VocabularyExcelDto
		{
			FileName = "test.xlsx",
			FilePath = "test.xlsx",
			Entries = entries,
			ErrorMessage = ""
		};
		vocabularyExcelReader.LoadVocabulary("test.xlsx").Returns(vocabulary);

		return (settingsStore, vocabularyProgressStore, vocabularyReferenceStore, vocabularyExcelReader);
	}
	#endregion LoadSession

	#region AddVocabularyFile
	[Fact]
	public void AddVocabularyFile_WithValidFile_ReturnsVocabularyReferenceWithUpdatedSessions()
	{
		// Arrange
		var (settingsStore, vocabularyProgressStore, vocabularyReferenceStore, vocabularyExcelReader) = CreateMocksForAddVocabularyFile();

		var vocabulary = new VocabularyExcelDto
		{
			FileName = "test.xlsx",
			FilePath = "c:\\test.xlsx",
			Entries = [
				new Entry { RuText = "word1", EnText = "translation1" },
				new Entry { RuText = "word2", EnText = "translation2" }
			],
			ErrorMessage = ""
		};
		vocabularyExcelReader.LoadVocabulary("c:\\test.xlsx").Returns(vocabulary);

		var vocabularyProgress = new VocabularyProgressDto
		{
			Sessions = [
				new VocabularyProgressSession { LastUpdated = DateTime.Now, LearnedEntries = 5, TotalEntries = 10 },
				new VocabularyProgressSession { LastUpdated = DateTime.Now, LearnedEntries = 3, TotalEntries = 8 },
				new VocabularyProgressSession { LastUpdated = DateTime.Now, LearnedEntries = 2, TotalEntries = 6 }
			]
		};
		vocabularyProgressStore.Load("c:\\test.xlsx").Returns(vocabularyProgress);

		var learnService = new LearnService(settingsStore, vocabularyProgressStore, vocabularyReferenceStore, vocabularyExcelReader);

		// Act
		var result = learnService.AddVocabularyFile("c:\\test.xlsx");

		// Assert
		Assert.NotNull(result);
		Assert.Equal("test.xlsx", result.FileName);
		Assert.Equal("c:\\test.xlsx", result.FilePath);
		Assert.Empty(result.ErrorMessage);
		Assert.Equal(3, result.Sessions.Count);
		Assert.Equal(5, result.Sessions[0].LearnedEntries);
		Assert.Equal(10, result.Sessions[0].TotalEntries);
		vocabularyReferenceStore.Received(1).AddAndSave(Arg.Any<VocabularyReferenceModel>());
	}

	[Fact]
	public void AddVocabularyFile_WithExcelLoadingError_ReturnsVocabularyReferenceWithErrorMessage()
	{
		// Arrange
		var (settingsStore, vocabularyProgressStore, vocabularyReferenceStore, vocabularyExcelReader) = CreateMocksForAddVocabularyFile();

		var vocabulary = new VocabularyExcelDto
		{
			FileName = "test.xlsx",
			FilePath = "c:\\test.xlsx",
			Entries = [],
			ErrorMessage = "Failed to load Excel file"
		};
		vocabularyExcelReader.LoadVocabulary("c:\\test.xlsx").Returns(vocabulary);

		var learnService = new LearnService(settingsStore, vocabularyProgressStore, vocabularyReferenceStore, vocabularyExcelReader);

		// Act
		var result = learnService.AddVocabularyFile("c:\\test.xlsx");

		// Assert
		Assert.NotNull(result);
		Assert.Equal("Failed to load Excel file", result.ErrorMessage);
		vocabularyReferenceStore.Received(1).AddAndSave(Arg.Any<VocabularyReferenceModel>());
	}

	[Fact]
	public void AddVocabularyFile_WithNoEntries_ReturnsVocabularyReferenceWithErrorMessage()
	{
		// Arrange
		var (settingsStore, vocabularyProgressStore, vocabularyReferenceStore, vocabularyExcelReader) = CreateMocksForAddVocabularyFile();

		var vocabulary = new VocabularyExcelDto
		{
			FileName = "test.xlsx",
			FilePath = "c:\\test.xlsx",
			Entries = [],
			ErrorMessage = ""
		};
		vocabularyExcelReader.LoadVocabulary("c:\\test.xlsx").Returns(vocabulary);

		var learnService = new LearnService(settingsStore, vocabularyProgressStore, vocabularyReferenceStore, vocabularyExcelReader);

		// Act
		var result = learnService.AddVocabularyFile("c:\\test.xlsx");

		// Assert
		Assert.NotNull(result);
		Assert.Equal("Contains no entries.", result.ErrorMessage);
		vocabularyReferenceStore.Received(1).AddAndSave(Arg.Any<VocabularyReferenceModel>());
	}

	[Fact]
	public void AddVocabularyFile_WithDuplicateEntries_ReturnsVocabularyReferenceWithErrorMessage()
	{
		// Arrange
		var (settingsStore, vocabularyProgressStore, vocabularyReferenceStore, vocabularyExcelReader) = CreateMocksForAddVocabularyFile();

		var vocabulary = new VocabularyExcelDto
		{
			FileName = "test.xlsx",
			FilePath = "c:\\test.xlsx",
			Entries = [
				new Entry { RuText = "word1", EnText = "translation1" },
				new Entry { RuText = "word2", EnText = "translation2" },
				new Entry { RuText = "word1", EnText = "translation3" }
			],
			ErrorMessage = ""
		};
		vocabularyExcelReader.LoadVocabulary("c:\\test.xlsx").Returns(vocabulary);

		var learnService = new LearnService(settingsStore, vocabularyProgressStore, vocabularyReferenceStore, vocabularyExcelReader);

		// Act
		var result = learnService.AddVocabularyFile("c:\\test.xlsx");

		// Assert
		Assert.NotNull(result);
		Assert.Equal("Duplicates: word1", result.ErrorMessage);
		vocabularyReferenceStore.Received(1).AddAndSave(Arg.Any<VocabularyReferenceModel>());
	}

	static (ISettingsStore, IVocabularyProgressStore, IVocabularyReferenceService, IVocabularyExcelReader) CreateMocksForAddVocabularyFile()
	{
		var settingsStore = Substitute.For<ISettingsStore>();
		var vocabularyProgressStore = Substitute.For<IVocabularyProgressStore>();
		var vocabularyReferenceStore = Substitute.For<IVocabularyReferenceService>();
		var vocabularyExcelReader = Substitute.For<IVocabularyExcelReader>();

		var settings = new SettingsDto
		{
			Learn = new LearnSettings
			{
				ExerciseSize = 5,
				CorrectAnswersToLearn = 2
			}
		};
		settingsStore.Load().Returns(settings);

		var defaultProgress = new VocabularyProgressDto
		{
			Sessions = [
				new VocabularyProgressSession(),
				new VocabularyProgressSession(),
				new VocabularyProgressSession()
			]
		};
		vocabularyProgressStore.Load(Arg.Any<string>()).Returns(defaultProgress);

		return (settingsStore, vocabularyProgressStore, vocabularyReferenceStore, vocabularyExcelReader);
	}
	#endregion AddVocabularyFile
}
