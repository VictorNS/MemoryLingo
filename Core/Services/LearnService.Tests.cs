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
	[Fact]
	public void LoadSession0_WithContinueTrue_CopiesFiveEntriesFromVocabularyProgress()
	{
		// Arrange
		var settingsStore = Substitute.For<ISettingsStore>();
		var vocabularyProgressStore = Substitute.For<IVocabularyProgressStore>();
		var vocabularyReferenceStore = Substitute.For<IVocabularyReferenceStore>();
		var vocabularyExcelReader = Substitute.For<IVocabularyExcelReader>();

		// Create test settings
		var settings = new SettingsDto
		{
			Learn = new LearnSettings
			{
				ExerciseSize = 5,
				CorrectAnswersToLearn = 2
			}
		};
		settingsStore.Load().Returns(settings);

		// Create test vocabulary
		var vocabulary = new VocabularyExcelDto
		{
			FileName = "test.xlsx",
			FilePath = "test.xlsx",
			Entries = [
				new Entry { RuText = "word1", EnText = "translation1" },
				new Entry { RuText = "word2", EnText = "translation2" },
				new Entry { RuText = "word3", EnText = "translation3" },
				new Entry { RuText = "word4", EnText = "translation4" },
				new Entry { RuText = "word5", EnText = "translation5" },
				new Entry { RuText = "word6", EnText = "translation6" },
			],
			ErrorMessage = ""
		};
		vocabularyExcelReader.LoadVocabulary("test.xlsx").Returns(vocabulary);

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
		var settingsStore = Substitute.For<ISettingsStore>();
		var vocabularyProgressStore = Substitute.For<IVocabularyProgressStore>();
		var vocabularyReferenceStore = Substitute.For<IVocabularyReferenceStore>();
		var vocabularyExcelReader = Substitute.For<IVocabularyExcelReader>();

		// Create test settings
		var settings = new SettingsDto
		{
			Learn = new LearnSettings
			{
				ExerciseSize = 5,
				CorrectAnswersToLearn = 2
			}
		};
		settingsStore.Load().Returns(settings);

		// Create test vocabulary
		var vocabulary = new VocabularyExcelDto
		{
			FileName = "test.xlsx",
			FilePath = "test.xlsx",
			Entries = [
				new Entry { RuText = "word1", EnText = "translation1" },
				new Entry { RuText = "word2", EnText = "translation2" },
				new Entry { RuText = "word3", EnText = "translation3" },
				new Entry { RuText = "word4", EnText = "translation4" },
				new Entry { RuText = "word5", EnText = "translation5" },
				new Entry { RuText = "word6", EnText = "translation6" },
			],
			ErrorMessage = ""
		};
		vocabularyExcelReader.LoadVocabulary("test.xlsx").Returns(vocabulary);

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
		var settingsStore = Substitute.For<ISettingsStore>();
		var vocabularyProgressStore = Substitute.For<IVocabularyProgressStore>();
		var vocabularyReferenceStore = Substitute.For<IVocabularyReferenceStore>();
		var vocabularyExcelReader = Substitute.For<IVocabularyExcelReader>();

		// Create test settings with default 30% for session 2
		var settings = new SettingsDto
		{
			Learn = new LearnSettings
			{
				ExerciseSize = 5,
				CorrectAnswersToLearn = 2,
				DifficultEntriesSession2Percent = 30
			}
		};
		settingsStore.Load().Returns(settings);

		// Create test vocabulary with 7 entries
		var vocabulary = new VocabularyExcelDto
		{
			FileName = "test.xlsx",
			FilePath = "test.xlsx",
			Entries = [
				new Entry { RuText = "word1", EnText = "translation1" },
				new Entry { RuText = "word2", EnText = "translation2" },
				new Entry { RuText = "word3", EnText = "translation3" },
				new Entry { RuText = "word4", EnText = "translation4" },
				new Entry { RuText = "word5", EnText = "translation5" },
				new Entry { RuText = "word6", EnText = "translation6" },
				new Entry { RuText = "word7", EnText = "translation7" },
			],
			ErrorMessage = ""
		};
		vocabularyExcelReader.LoadVocabulary("test.xlsx").Returns(vocabulary);

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
		var settingsStore = Substitute.For<ISettingsStore>();
		var vocabularyProgressStore = Substitute.For<IVocabularyProgressStore>();
		var vocabularyReferenceStore = Substitute.For<IVocabularyReferenceStore>();
		var vocabularyExcelReader = Substitute.For<IVocabularyExcelReader>();

		// Create test settings with 50% for session 3
		var settings = new SettingsDto
		{
			Learn = new LearnSettings
			{
				ExerciseSize = 5,
				CorrectAnswersToLearn = 2,
				DifficultEntriesSession3Percent = 50
			}
		};
		settingsStore.Load().Returns(settings);

		// Create test vocabulary with 6 entries
		var vocabulary = new VocabularyExcelDto
		{
			FileName = "test.xlsx",
			FilePath = "test.xlsx",
			Entries = [
				new Entry { RuText = "word1", EnText = "translation1" },
				new Entry { RuText = "word2", EnText = "translation2" },
				new Entry { RuText = "word3", EnText = "translation3" },
				new Entry { RuText = "word4", EnText = "translation4" },
				new Entry { RuText = "word5", EnText = "translation5" },
				new Entry { RuText = "word6", EnText = "translation6" },
			],
			ErrorMessage = ""
		};
		vocabularyExcelReader.LoadVocabulary("test.xlsx").Returns(vocabulary);

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
}
