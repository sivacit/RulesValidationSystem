SOLUTION=RulesValidationSystem.sln
SERVICES=src/Services
DOCKER_COMPOSE=docker/docker-compose.yml

# Default target
.PHONY: help
help:
	@echo "Usage:"
	@echo "  make build         - Build the solution"
	@echo "  make run           - Run all services"
	@echo "  make test          - Run all tests"
	@echo "  make clean         - Clean build artifacts"
	@echo "  make docker-up     - Run Docker Compose"
	@echo "  make docker-down   - Stop Docker Compose"
	@echo "  make ef-migrations - Add EF migrations"

.PHONY: setup
setup:
	dotnet tool install --global dotnet-ef
.PHONY: build
build:
	dotnet build $(SOLUTION)

.PHONY: run
run:
	dotnet run --project $(SERVICES)/FileProcessor.API/FileProcessor.API.csproj &
	dotnet run --project $(SERVICES)/RuleManager.API/RuleManager.API.csproj &
	dotnet run --project $(SERVICES)/Lookup.API/Lookup.API.csproj

.PHONY: test
test:
	dotnet test

.PHONY: clean
clean:
	dotnet clean

.PHONY: docker-up
docker-up:
	docker-compose -f $(DOCKER_COMPOSE) up --build -d

.PHONY: docker-down
docker-down:
	docker-compose -f $(DOCKER_COMPOSE) down

.PHONY: ef-migrations
ef-migrations:
	dotnet ef migrations add InitialCreate --project $(SERVICES)/FileProcessor.API/FileProcessor.API.csproj --startup-project $(SERVICES)/FileProcessor.API
