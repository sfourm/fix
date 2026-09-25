data "aws_caller_identity" "current" {}
data "aws_partition" "current" {}

locals {
  account_id = data.aws_caller_identity.current.account_id
  partition  = data.aws_partition.current.partition
  ssm_params = "arn:${local.partition}:ssm:${var.aws_region}:${local.account_id}:parameter${local.ssm_prefix}/*"
}

# ---------- Role do nó: SSM Session Manager/Run Command + leitura dos segredos da aplicação ----------

resource "aws_iam_role" "node" {
  name = "${local.name}-node"
  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Effect    = "Allow"
      Principal = { Service = "ec2.amazonaws.com" }
      Action    = "sts:AssumeRole"
    }]
  })
}

resource "aws_iam_role_policy_attachment" "node_ssm" {
  role       = aws_iam_role.node.name
  policy_arn = "arn:${local.partition}:iam::aws:policy/AmazonSSMManagedInstanceCore"
}

resource "aws_iam_role_policy" "node_parameters" {
  name = "read-fix-parameters"
  role = aws_iam_role.node.id
  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Sid      = "ReadFixParameters"
        Effect   = "Allow"
        Action   = ["ssm:GetParameter", "ssm:GetParameters"]
        Resource = local.ssm_params
      },
      {
        Sid      = "PublishKubeconfig"
        Effect   = "Allow"
        Action   = "ssm:PutParameter"
        Resource = aws_ssm_parameter.kubeconfig.arn
      },
      {
        Sid      = "DecryptViaSsm"
        Effect   = "Allow"
        Action   = ["kms:Decrypt", "kms:Encrypt", "kms:GenerateDataKey"]
        Resource = "*"
        Condition = {
          StringEquals = { "kms:ViaService" = "ssm.${var.aws_region}.amazonaws.com" }
        }
      }
    ]
  })
}

resource "aws_iam_instance_profile" "node" {
  name = "${local.name}-node"
  role = aws_iam_role.node.name
}
