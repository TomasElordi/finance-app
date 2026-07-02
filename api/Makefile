build-FinanceApi:
	dotnet publish -c Release -r linux-x64 --self-contained true -o $(ARTIFACTS_DIR)
	mv $(ARTIFACTS_DIR)/api $(ARTIFACTS_DIR)/bootstrap
