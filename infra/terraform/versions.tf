terraform {
  required_version = ">= 1.6"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 6.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.6"
    }
  }

  # Estado local por padrão (apresentação). Para equipe, use um bucket S3:
  # backend "s3" {
  #   bucket       = "<bucket-de-estado>"
  #   key          = "fix/presentation/terraform.tfstate"
  #   region       = "us-east-1"
  #   use_lockfile = true
  # }
}

# Credenciais do AWS CLI (aws configure / AWS_PROFILE / SSO).
provider "aws" {
  region  = var.aws_region
  profile = var.aws_profile

  default_tags {
    tags = {
      Project     = "fix"
      Environment = var.environment
      ManagedBy   = "terraform"
    }
  }
}
