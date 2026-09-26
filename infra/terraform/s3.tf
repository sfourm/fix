# ---------- Arquivos enviados pela tela de Uploads (storage-service) ----------
# Bucket privado e criptografado. O storage-service acessa com um usuário IAM restrito a este bucket: os pods não
# alcançam as credenciais do nó (IMDSv2 com hop limit 1), então a chave vai para o SSM e o fix-sync a entrega no
# Secret fix-secrets (s3-access-key, s3-secret-key, s3-bucket, s3-region). O navegador baixa pelo BFF, nunca direto.

resource "aws_s3_bucket" "files" {
  # Nome global: o ID da conta evita colisão com buckets de outras contas.
  bucket = "${local.name}-files-${local.account_id}"
  tags   = { Name = "${local.name}-files" }
}

resource "aws_s3_bucket_public_access_block" "files" {
  bucket                  = aws_s3_bucket.files.id
  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

resource "aws_s3_bucket_ownership_controls" "files" {
  bucket = aws_s3_bucket.files.id
  rule {
    object_ownership = "BucketOwnerEnforced"
  }
}

resource "aws_s3_bucket_server_side_encryption_configuration" "files" {
  bucket = aws_s3_bucket.files.id
  rule {
    apply_server_side_encryption_by_default {
      sse_algorithm = "AES256"
    }
  }
}

resource "aws_s3_bucket_lifecycle_configuration" "files" {
  bucket = aws_s3_bucket.files.id
  rule {
    id     = "abort-incomplete-uploads"
    status = "Enabled"
    filter {}
    abort_incomplete_multipart_upload {
      days_after_initiation = 1
    }
  }
}

# Só HTTPS.
resource "aws_s3_bucket_policy" "files" {
  bucket = aws_s3_bucket.files.id
  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Sid       = "DenyInsecureTransport"
      Effect    = "Deny"
      Principal = "*"
      Action    = "s3:*"
      Resource  = [aws_s3_bucket.files.arn, "${aws_s3_bucket.files.arn}/*"]
      Condition = { Bool = { "aws:SecureTransport" = "false" } }
    }]
  })
  depends_on = [aws_s3_bucket_public_access_block.files]
}

resource "aws_iam_user" "storage" {
  name = "${local.name}-storage"
  tags = { Purpose = "storage-service - uploads do cluster" }
}

resource "aws_iam_user_policy" "storage" {
  name = "fix-files-bucket"
  user = aws_iam_user.storage.name
  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Sid      = "ListBucket"
        Effect   = "Allow"
        Action   = ["s3:ListBucket", "s3:GetBucketLocation"]
        Resource = aws_s3_bucket.files.arn
      },
      {
        Sid      = "ReadWriteObjects"
        Effect   = "Allow"
        Action   = ["s3:PutObject", "s3:GetObject", "s3:DeleteObject", "s3:AbortMultipartUpload"]
        Resource = "${aws_s3_bucket.files.arn}/*"
      }
    ]
  })
}

resource "aws_iam_access_key" "storage" {
  user = aws_iam_user.storage.name
}

resource "aws_ssm_parameter" "s3_bucket" {
  name  = "${local.ssm_prefix}/s3-bucket"
  type  = "String"
  value = aws_s3_bucket.files.bucket
}

resource "aws_ssm_parameter" "s3_region" {
  name  = "${local.ssm_prefix}/s3-region"
  type  = "String"
  value = var.aws_region
}

resource "aws_ssm_parameter" "s3_access_key" {
  name  = "${local.ssm_prefix}/s3-access-key"
  type  = "SecureString"
  value = aws_iam_access_key.storage.id
}

resource "aws_ssm_parameter" "s3_secret_key" {
  name  = "${local.ssm_prefix}/s3-secret-key"
  type  = "SecureString"
  value = aws_iam_access_key.storage.secret
}
