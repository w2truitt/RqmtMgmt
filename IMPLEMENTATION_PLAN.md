# Hierarchical Subsections Implementation Plan

## Current Status
- RqmtMgmtShared version: 1.0.38
- Latest migration: 20250916192604_UpdateRequirementTypeEnumValues

## Implementation Phases

### Phase 1: RqmtMgmtShared Updates (CURRENT)
- [x] Update DocumentSectionDto with hierarchy fields
- [x] Increment version to 1.0.39
- [x] Build NuGet package
- [x] Copy to local_nuget directories

### Phase 2: Backend Model & Migration
- [ ] Update DocumentSection model
- [ ] Create database migration
- [ ] Update DbContext configuration
- [ ] Run migration locally

### Phase 3: Backend Service Layer
- [ ] Update DocumentSectionService for hierarchical queries
- [ ] Add validation for circular references
- [ ] Update reordering logic
- [ ] Add helper methods for tree operations

### Phase 4: Backend API Endpoints
- [ ] Update GET endpoints to include children
- [ ] Update POST/PUT for hierarchy validation
- [ ] Update DELETE for cascade handling
- [ ] Test API endpoints

### Phase 5: Frontend Components
- [ ] Update DocumentSections component for tree view
- [ ] Add expand/collapse functionality
- [ ] Update drag-and-drop for nesting
- [ ] Add indent/outdent actions

### Phase 6: Testing & Validation
- [ ] Unit tests for service layer
- [ ] API integration tests
- [ ] E2E tests for UI
- [ ] Performance testing

### Phase 7: Documentation & Migration
- [ ] Update API documentation
- [ ] Update user guide
- [ ] Create migration guide for existing data
- [ ] Update population scripts

## Commands to Execute

### Build and Deploy NuGet Package
```bash
cd /home/wtruitt/src/repos/RqmtMgmt/RqmtMgmtShared
dotnet pack -c Release
cp bin/Release/RqmtMgmtShared.1.0.39.nupkg ../local_nuget/
cp bin/Release/RqmtMgmtShared.1.0.39.nupkg ../backend/local_nuget/
cp bin/Release/RqmtMgmtShared.1.0.39.nupkg ../frontend/local_nuget/
```

### Create Migration
```bash
cd /home/wtruitt/src/repos/RqmtMgmt/backend
dotnet ef migrations add AddHierarchicalSections
```

### Apply Migration
```bash
dotnet ef database update
```

## Status: STARTING PHASE 1
