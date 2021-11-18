<?php
	$u_id = $_POST["Input_user"];

	$con = mysqli_connect("localhost", "jinone12", "wlsdnjs12!!", "jinone12");

	if(!$con)
		die("Could not Connect".mysqli_connect_error());

	$check = mysqli_query($con, "SELECT user_id FROM GawiBawiBo WHERE user_id ='".$u_id."'");

	$numrows = mysqli_num_rows($check);
	if(!$check || $numrows == 0)
	{  		
		die("ID Does Exist.");
	} 
	
	$JSONBUFF = array();
	$sqlList = mysqli_query($con ,"SELECT * FROM GawiBawiBo ORDER BY best_score DESC LIMIT 0,10");
	
	$rowsCount = mysqli_num_rows($sqlList);
	if(!$sqlList || $rowsCount == 0)
	{
		die("List does not exist\n");	
	}
	
	$RowDatas = array();
	$Return = array();
	for($aa = 0; $aa < $rowsCount; $aa++)
	{
		$a_row = mysqli_fetch_array($sqlList);
		if($a_row != false)
		{
			$RowDatas["user_id"] = $a_row["user_id"];
			$RowDatas["nick_name"] = $a_row["nick_name"];
			$RowDatas["best_score"] = $a_row["best_score"];
			array_push($Return,$RowDatas);		
		}
	}
	$JSONBUFF["RkList"] = $Return;	

	//---------------------------- 자신의 랭킹 순위 찾아오기...
	//그룹화하여 데이터 조회 (GROUP BY) https://extbrain.tistory.com/56

	 $check = mysqli_query($con, "SELECT user_id, myrankidx 
   		FROM (SELECT user_id, 
    		rank() over(ORDER BY best_score DESC) as myrankidx
    		FROM GawiBawiBo) as CNT 
   		WHERE user_id='".$u_id."'");

	$numrow = mysqli_num_rows($check);
	if (!$check || $numrow == 0)
	{
		die("Ranking search failed for ID. \n");
	}

	if($row = mysqli_fetch_assoc($check))
	{	  
		//JSON 파일 생성
		$JSONBUFF["my_rank"]   = $row["myrankidx"];   
		//header("Content-type:application/json"); //생략
		$output = json_encode($JSONBUFF, JSON_UNESCAPED_UNICODE); //한글 포함된 경우
		echo $output;
		echo ("\n");
		echo "Get_Rank_list_Success";
	}
?>