import sys
from pathlib import Path
sys.path.append(str(Path(__file__).resolve().parents[1]))
from spark_job import run_job

if __name__ == "__main__":
    if len(sys.argv) > 1:
        source_table_name = sys.argv[1].upper()
        print(f"Processing table: {source_table_name}")
        run_job(source_table_name)
    else:
        print("Source table: `spark-submit main.py dbo.TXFOOTER OR GCEN509.ARTCAISSE`")
        sys.exit(1)