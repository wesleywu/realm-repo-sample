DOTNET = dotnet
SOLUTION_NAME = RealmRepoSample
PROJECT_NAME = RealmRepo
TEST_PROJECT_NAME = RealmRepo.Test
SLN = $(SOLUTION_NAME).sln

.PHONY: test

test:
	@$(DOTNET) test $(SLN)
