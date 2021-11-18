<?php
	$u_id = $_POST["Input_user"]; 
	$Quick_Item = $_POST["Quick_Item"];
	$Quick_Item_Num = $_POST["Quick_Item_Num"];
	$Quick_Skill = $_POST["Quick_Skill"];

	$con = mysqli_connect("localhost", "jinone12", "wlsdnjs12!!", "jinone12");

	if(!$con)
		die("Could not Connect".mysqli_connect_error());
	$check = mysqli_query($con, "SELECT user_id FROM 3DPortfolio WHERE user_id ='".$u_id."'");

	$numrows = mysqli_num_rows($check);
	if($numrows == 0)
	{  		
		die("ID Does Exist.");
	} 
	if($row = mysqli_fetch_assoc($check))
	{
		mysqli_query($con, "UPDATE 3DPortfolio SET `quick_item`='".$Quick_Item."' WHERE `user_id`='".$u_id."'");
		mysqli_query($con, "UPDATE 3DPortfolio SET `quick_item_num`='".$Quick_Item_Num."' WHERE `user_id`='".$u_id."'");
		mysqli_query($con, "UPDATE 3DPortfolio SET `quick_skill`='".$Quick_Skill."' WHERE `user_id`='".$u_id."'");
		echo("UpdateSuccess");
	}
	mysqli_close($con);
?>