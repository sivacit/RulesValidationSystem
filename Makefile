SOLUTION=RulesValidationSystem.sln
SERVICES=src/Services
DOCKER_COMPOSE=docker/docker-compose.yml

# Define project paths
FILEPROCESS_API=$(SERVICES)/FileProcessor.API
RULEENGINE_API=$(SERVICES)/RuleManager.API/
WEB_UI=src/Web/UI

# Run both services
run:
	@echo "Running FileProcess.API..."
	@cd $(FILEPROCESS_API) && dotnet run &

	@echo "Running RuleEngine.API..."
	@cd $(RULEENGINE_API) && dotnet run &
	
	@echo "Running Web UI..."
	cd $(WEB_UI) && npm install && npm run start &
	@wait

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
	dotnet dev-certs https --trust
.PHONY: build
build:
	dotnet build $(SOLUTION)

.PHONY: run
run:
	dotnet run --project $(FILEPROCESS_API)/FileProcessor.API.csproj &
	dotnet run --project $(RULEENGINE_API)/RuleManager.API.csproj &
	cd $(WEB_UI) && npm install && npm run start &

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
