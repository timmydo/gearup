<#
.SYNOPSIS
  This script deploys the GearUp app.

.DESCRIPTION
  <Brief description of script>

.PARAMETER Name
    Name of the app

.PARAMETER Location
    Azure location (e.g. eastus)

.PARAMETER Tree
    Source code tree from github

.INPUTS
  None

.OUTPUTS
  None
  
.EXAMPLE
  .\deploy.ps1 -Name gearupdeploytest -Location eastus -Tree beta5
  
#>

Param (
	[Parameter(Mandatory=$true)]
	$name,
	[Parameter(Mandatory=$true)]
	$location,
	[Parameter(Mandatory=$true)]
	$tree
)


$params = @{}
$params.Add("subscriptionId", (Get-AzureSubscription -Default).SubscriptionId)
$params.Add("farmResourceGroup", $name)
$params.Add("siteName", $name)
$params.Add("redisName", $name)
$params.Add("docdbName", $name)
$params.Add("searchName", $name)
$params.Add("searchIndexName", $name)
$params.Add("storageName", $name)
$params.Add("storageAccountType", "Standard-GRS")
$params.Add("hostingPlanName", "MyHostingPlan")
$params.Add("location", $location)
$params.Add("repoUrl", "https://github.com/timmydo/gearup.git")
$params.Add("webSku", "Free")
$params.Add("workerSize", "small")
$params.Add("branch", $tree)

write-verbose ($params|out-string)

function Publish() {
	write-verbose "Switching to AzureResourceManager mode..."
	Switch-AzureMode -Name AzureResourceManager
	write-verbose "Creating Resource $name"
	New-AzureResourceGroup -Name $name -Location $location
	New-AzureResourceGroupDeployment -ResourceGroupName $name -TemplateFile azuredeploy.json `
	    -TemplateParameterObject $params
		#-TemplateParameterFile defaultSettings.json `
	
}



Publish