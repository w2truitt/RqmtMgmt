#!/bin/bash

# E2E Test Logger Refactoring Script
# Transforms Output.WriteLine calls to use TestLogger

set -e

DRY_RUN=false
BACKUP_FILES=false

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --dry-run)
            DRY_RUN=true
            shift
            ;;
        --backup)
            BACKUP_FILES=true
            shift
            ;;
        -h|--help)
            echo "Usage: $0 [--dry-run] [--backup]"
            echo "  --dry-run  Show what changes would be made without modifying files"
            echo "  --backup   Create .bak files before making changes"
            exit 0
            ;;
        *)
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
done

echo "E2E Test Logger Refactoring Script"
echo "================================="

if [ "$DRY_RUN" = true ]; then
    echo "DRY RUN MODE - No files will be modified"
fi

if [ "$BACKUP_FILES" = true ] && [ "$DRY_RUN" = false ]; then
    echo "BACKUP MODE - .bak files will be created"
fi

echo ""

# Check if we're in the right directory
if [ ! -d "frontend.E2ETests/Workflows" ]; then
    echo "Error: Workflows directory not found"
    echo "Please run this script from the repository root directory."
    exit 1
fi

# Function to determine the appropriate logger method
get_logger_method() {
    local message="$1"
    local lower_message=$(echo "$message" | tr '[:upper:]' '[:lower:]')
    
    # Authentication-related messages
    if echo "$lower_message" | grep -q -E "(authenticated|authentication|login|session|token|cache)"; then
        echo "TestLogger.LogAuthentication"
        return
    fi
    
    # Debug information
    if echo "$lower_message" | grep -q -E "(debug|found|current|page|url|title|element|input|form|button)"; then
        echo "TestLogger.LogDebug"
        return
    fi
    
    # Error messages
    if echo "$lower_message" | grep -q -E "(error|failed|exception|invalid|missing)"; then
        echo "TestLogger.LogError"
        return
    fi
    
    # Default to test step
    echo "TestLogger.LogTestStep"
}

# Function to transform a single file
transform_file() {
    local file_path="$1"
    echo "Processing: $file_path"
    
    local temp_file=$(mktemp)
    local changes=0
    
    # Check if file already has the using statement
    if ! grep -q "using frontend.E2ETests.Infrastructure;" "$file_path"; then
        # Find the last using statement and add our using after it
        awk '
            /^using / { last_using = NR; using_lines[NR] = $0; next }
            last_using && NR == last_using + 1 && !added {
                for (i = 1; i <= last_using; i++) {
                    if (using_lines[i]) print using_lines[i]
                }
                print "using frontend.E2ETests.Infrastructure;"
                print $0
                added = 1
                next
            }
            { print }
        ' "$file_path" > "$temp_file"
        
        mv "$temp_file" "$file_path.tmp"
        echo "  Added using statement for TestLogger"
        ((changes++))
    else
        cp "$file_path" "$file_path.tmp"
    fi
    
    # Transform Output.WriteLine calls
    while IFS= read -r line; do
        if echo "$line" | grep -q "Output\.WriteLine("; then
            # Extract the message content
            message=$(echo "$line" | sed -n 's/.*Output\.WriteLine(\([^)]*\));.*/\1/p')
            
            # Determine the appropriate logger method
            logger_method=$(get_logger_method "$message")
            
            # Create the replacement
            replacement="        $logger_method($message, Output);"
            
            # Replace the line
            echo "$replacement"
            echo "    Changed: Output.WriteLine($message); -> $logger_method($message, Output);"
            ((changes++))
        else
            echo "$line"
        fi
    done < "$file_path.tmp" > "$temp_file"
    
    if [ $changes -gt 0 ]; then
        echo "  Found $changes changes"
        
        if [ "$DRY_RUN" = false ]; then
            if [ "$BACKUP_FILES" = true ]; then
                cp "$file_path" "$file_path.bak"
                echo "  Created backup: $file_path.bak"
            fi
            
            mv "$temp_file" "$file_path"
            echo "  File updated successfully!"
        else
            echo "  [DRY RUN] File would be updated"
            rm "$temp_file"
        fi
    else
        echo "  No changes needed"
        rm "$temp_file"
    fi
    
    rm -f "$file_path.tmp"
    echo ""
}

# Process all C# files in the Workflows directory
file_count=0
for file in frontend.E2ETests/Workflows/*.cs; do
    if [[ -f "$file" && ! "$file" =~ TestBase ]]; then
        transform_file "$file"
        ((file_count++))
    fi
done

echo "Processed $file_count files"
echo "Refactoring complete!"

if [ "$DRY_RUN" = true ]; then
    echo ""
    echo "To apply these changes, run the script without --dry-run:"
    echo "  ./refactor-to-testlogger.sh"
    echo ""
    echo "To apply changes with backups:"
    echo "  ./refactor-to-testlogger.sh --backup"
fi