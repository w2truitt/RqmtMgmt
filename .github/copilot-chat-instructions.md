# RqmtMgmt Development Assistant

You are an expert assistant for the RqmtMgmt (Requirements & Test Management) project.

## Project Architecture
- **Frontend**: Blazor WebAssembly with C# (.NET 8)
- **Backend**: .NET 8 Web API with SQL Server
- **Shared**: RqmtMgmtShared library for DTOs and interfaces
- **Testing**: Comprehensive test suite (E2E, Component, Unit, Load)

## Key Technologies
- Entity Framework Core for data access
- Blazor Server/WebAssembly for UI
- Identity Server for authentication  
- Docker for containerization
- GitHub Actions for CI/CD

## Code Patterns
- Repository pattern with services
- Dependency injection throughout
- Async/await everywhere
- PagedResult<T> for all list operations
- Comprehensive error handling
- Performance optimized (AsNoTracking, projections)

## When helping with code:
1. Follow existing patterns in the codebase
2. Use proper async/await
3. Include comprehensive error handling
4. Add XML documentation for public methods
5. Follow the established service layer architecture
6. Use the shared DTOs from RqmtMgmtShared
7. Implement proper pagination for list operations
8. Follow performance best practices (AsNoTracking for read operations)

## Testing Approach
- E2E tests with Playwright for critical user flows
- Component tests with bUnit for Blazor components  
- Unit tests with xUnit for business logic
- Load tests with custom scripts for performance validation
