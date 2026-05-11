import { ethers } from "hardhat";

async function main() {
  console.log("Deploying TransactionLog contract...");

  const [deployer] = await ethers.getSigners();
  console.log("Deploying with account:", deployer.address);

  const TransactionLog = await ethers.getContractFactory("TransactionLog");

  const contract = await TransactionLog.deploy();
  await contract.waitForDeployment();

  const address = await contract.getAddress();

  console.log("");
  console.log("TransactionLog deployed successfully.");
  console.log("Contract address:", address);

  console.log("");
  console.log("For Docker Compose:");
  console.log("Add or update this value in your root .env file:");
  console.log(`BLOCKCHAIN_CONTRACT_ADDRESS=${address}`);

  console.log("");
  console.log("For local backend run:");
  console.log("Add or update this value in wdb-backend/appsettings.Development.json:");
  console.log(`"ContractAddress": "${address}"`);

  console.log("");
  console.log("Then restart your backend.");
}

main().catch((error) => {
  console.error(error);
  process.exitCode = 1;
});
