spark-submit \
  --driver-memory 2g \
  --conf spark.driver.cores=2 \
  --executor-memory 6g \
  --conf spark.executor.memoryOverhead=1g \
  --conf spark.executor.cores=2 \
  --conf spark.executor.instances=2 \
  --conf spark.kubernetes.executor.request.cores=2 \
  --conf spark.kubernetes.executor.limit.cores=2 spark_job.py