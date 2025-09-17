#!/bin/bash

# Add document services injection after line 4
sed -i '4a@inject frontend.Services.DocumentsDataService DocumentsService\n@inject frontend.Services.DocumentSectionsDataService DocumentSectionsService' frontend/Pages/ProjectRequirements.razor

# Update search controls layout - change col-md-6 to col-md-4
sed -i 's/col-md-6/col-md-4/' frontend/Pages/ProjectRequirements.razor

# Add document filter dropdown before the type filter
sed -i '/col-md-3.*select.*filterType/i\        <div class="col-md-2">\
            <select class="form-select" @bind="selectedDocumentId" @bind:after="FilterByDocument">\
                <option value="">All Documents</option>\
                <option value="0">Standalone Only</option>\
                @if (availableDocuments != null)\
                {\
                    @foreach (var doc in availableDocuments)\
                    {\
                        <option value="@doc.Id">@doc.Type - @doc.Title</option>\
                    }\
                }\
            </select>\
        </div>' frontend/Pages/ProjectRequirements.razor

# Add Document Context column header
sed -i 's/<th scope="col" style="width: 150px;">Created<\/th>/<th scope="col" style="width: 180px;">Document Context<\/th>\
                                <th scope="col" style="width: 150px;">Created<\/th>/' frontend/Pages/ProjectRequirements.razor

# Add document context cell in table row
sed -i 's/<td class="align-middle">\
                                        <small class="text-muted">@requirement.CreatedAt.ToString("MMM dd, yyyy")<\/small>\
                                    <\/td>/<td class="align-middle">\
                                        <RequirementDocumentContext Requirement="@requirement" Documents="@availableDocuments" Sections="@availableSections" \/>\
                                    <\/td>\
                                    <td class="align-middle">\
                                        <small class="text-muted">@requirement.CreatedAt.ToString("MMM dd, yyyy")<\/small>\
                                    <\/td>/' frontend/Pages/ProjectRequirements.razor

echo "File updated successfully"