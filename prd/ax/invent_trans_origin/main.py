import sys
from pathlib import Path
sys.path.append(str(Path(__file__).resolve().parents[1]))
from spark_job import run_job

SOURCE_TABLE = "dbo.INVENTTRANSORIGIN"

if __name__ == "__main__":
    run_job(SOURCE_TABLE)
