<# Custom Script for Windows #>
function Create-EventSources() {
    $eventSources = @("Stateless1" )
    foreach ($source in $eventSources) {
            if ([System.Diagnostics.EventLog]::SourceExists($source) -eq $false) {
                [System.Diagnostics.EventLog]::CreateEventSource($source, "Application")
            }
    }
}
Create-EventSources